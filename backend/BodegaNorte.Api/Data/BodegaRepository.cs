using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BodegaNorte.Api.Models;
using BodegaNorte.Api.Services;
using Npgsql;
using NpgsqlTypes;

namespace BodegaNorte.Api.Data;

public sealed class BodegaRepository : IAsyncDisposable
{
    private readonly NpgsqlDataSource _source;
    private readonly string _schema;
    private readonly string _bootstrapEmail;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public BodegaRepository(string connectionString, IConfiguration configuration)
    {
        _schema = configuration["Database:Schema"] ?? "bodega_norte";
        if (!Regex.IsMatch(_schema, "^[a-z][a-z0-9_]{0,62}$")) throw new InvalidOperationException("Nombre de esquema inválido.");
        _bootstrapEmail = configuration["BootstrapAdminEmail"]?.Trim().ToLowerInvariant() ?? "";
        _source = NpgsqlDataSource.Create(connectionString);
    }
    public ValueTask DisposeAsync() => _source.DisposeAsync();
    private string Sql(string sql) => sql.Replace("bodega_norte.", $"\"{_schema}\".");
    private NpgsqlCommand Command(string sql, NpgsqlConnection connection, NpgsqlTransaction? tx, params (string Name, object? Value)[] args)
    {
        var command = new NpgsqlCommand(Sql(sql), connection, tx);
        foreach (var (name, value) in args)
            command.Parameters.Add(value is null ? new NpgsqlParameter(name, NpgsqlDbType.Text) { Value = DBNull.Value } : new NpgsqlParameter(name, value));
        return command;
    }
    private async Task<int> Execute(string sql, NpgsqlConnection c, NpgsqlTransaction? tx, CancellationToken ct, params (string, object?)[] args)
    {
        await using var command = Command(sql, c, tx, args);
        return await command.ExecuteNonQueryAsync(ct);
    }
    private async Task<object?> Scalar(string sql, NpgsqlConnection c, NpgsqlTransaction? tx, CancellationToken ct, params (string, object?)[] args)
    {
        await using var command = Command(sql, c, tx, args);
        return await command.ExecuteScalarAsync(ct);
    }
    private async Task<List<T>> Query<T>(string sql, NpgsqlConnection c, NpgsqlTransaction? tx, CancellationToken ct,
        Func<NpgsqlDataReader, T> map, params (string, object?)[] args)
    {
        await using var command = Command(sql, c, tx, args);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var result = new List<T>();
        while (await reader.ReadAsync(ct)) result.Add(map(reader));
        return result;
    }
    private Task Audit(string action, string entity, string id, object details, Actor actor, NpgsqlConnection c, NpgsqlTransaction tx, CancellationToken ct) =>
        Execute("""
            INSERT INTO bodega_norte.audit_log(action,entity,entity_id,actor_id,actor_email,details)
            VALUES(@action,@entity,@id,@actor,@email,CAST(@details AS jsonb))
            """, c, tx, ct, ("action", action), ("entity", entity), ("id", id), ("actor", actor.Id), ("email", actor.Email), ("details", JsonSerializer.Serialize(details, Json)));
    private Task Movement(int id, decimal quantity, decimal balance, string type, string reason, Actor actor,
        NpgsqlConnection c, NpgsqlTransaction tx, CancellationToken ct) =>
        Execute("""
            INSERT INTO bodega_norte.stock_movements(product_id,quantity,balance,type,reason,actor_id,actor_email)
            VALUES(@id,@quantity,@balance,@type,@reason,@actor,@email)
            """, c, tx, ct, ("id", id), ("quantity", quantity), ("balance", balance), ("type", type), ("reason", reason), ("actor", actor.Id), ("email", actor.Email));

    public async Task<bool> TestConnectionAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        return Convert.ToInt32(await Scalar("SELECT 1", c, null, ct)) == 1;
    }

    public async Task<MemberDto?> GetMemberAsync(Guid id, string email, bool confirmed, CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        if (confirmed && email.Equals(_bootstrapEmail, StringComparison.OrdinalIgnoreCase) && _bootstrapEmail.Length > 0)
            await Execute("""
                INSERT INTO bodega_norte.members(user_id,email,role) VALUES(@id,@email,'admin')
                ON CONFLICT(user_id) DO NOTHING
                """, c, null, ct, ("id", id), ("email", email.ToLowerInvariant()));
        var rows = await Query("SELECT user_id,email,role,is_active FROM bodega_norte.members WHERE user_id=@id",
            c, null, ct, r => new MemberDto(r.GetGuid(0), r.GetString(1), r.GetString(2), r.GetBoolean(3)), ("id", id));
        return rows.SingleOrDefault();
    }

    private const string ProductsSql = """
        SELECT p.code,p.name,c.name,p.stock,p.min_stock,p.cost,p.sale_price,COALESCE(s.name,''),p.unit,p.updated_at,p.version
        FROM bodega_norte.products p JOIN bodega_norte.categories c USING(category_id)
        LEFT JOIN bodega_norte.suppliers s USING(supplier_id) WHERE p.is_active=true
        """;
    private static ProductDto MapProduct(NpgsqlDataReader r) => new(r.GetString(0), r.GetString(1), r.GetString(2),
        r.GetDecimal(3), r.GetDecimal(4), r.GetDecimal(5), r.GetDecimal(6), r.GetString(7), r.GetString(8), r.GetFieldValue<DateTimeOffset>(9), r.GetInt64(10));
    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        return await Query(ProductsSql + " ORDER BY p.code", c, null, ct, MapProduct);
    }

    public async Task<ProductDto> SaveProductAsync(string? code, ProductInput input, Actor actor, CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        var isNew = code is null;
        code ??= $"P-{Guid.NewGuid().ToString("N")[..20]}";
        var category = Convert.ToInt32(await Scalar("""
            INSERT INTO bodega_norte.categories(name) VALUES(@name)
            ON CONFLICT(name) DO UPDATE SET is_active=true RETURNING category_id
            """, c, tx, ct, ("name", input.Category.Trim())));
        var supplier = string.IsNullOrWhiteSpace(input.Supplier) ? null : await Scalar(
            "SELECT supplier_id FROM bodega_norte.suppliers WHERE name=@name AND is_active=true FOR SHARE",
            c, tx, ct, ("name", input.Supplier.Trim()));
        if (!string.IsNullOrWhiteSpace(input.Supplier) && supplier is null) throw new BusinessException("El proveedor no está disponible.");
        var args = new (string, object?)[] { ("code", code), ("name", input.Name.Trim()), ("category", category),
            ("supplier", supplier ?? DBNull.Value), ("unit", input.Unit.Trim()), ("stock", input.Stock),
            ("min", input.MinStock), ("cost", input.Cost), ("price", input.Price), ("version", input.ExpectedVersion ?? 0) };
        int id;
        if (isNew)
        {
            // No UPSERT: crear un producto nunca reemplaza otro.
            await using var command = Command("""
                INSERT INTO bodega_norte.products(code,name,category_id,supplier_id,unit,stock,min_stock,cost,sale_price)
                VALUES(@code,@name,@category,CAST(@supplier AS integer),@unit,@stock,@min,@cost,@price) RETURNING product_id
                """, c, tx, args);
            if (supplier is null) command.Parameters["supplier"].NpgsqlDbType = NpgsqlDbType.Integer;
            id = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
            await Movement(id, input.Stock, input.Stock, "initial", "Stock inicial", actor, c, tx, ct);
        }
        else
        {
            var current = await Query("SELECT product_id,stock,version FROM bodega_norte.products WHERE code=@code AND is_active=true FOR UPDATE",
                c, tx, ct, r => (Id: r.GetInt32(0), Stock: r.GetDecimal(1), Version: r.GetInt64(2)), ("code", code));
            if (current.Count == 0) throw new BusinessException("El producto no existe.", 404);
            if (current[0].Version != input.ExpectedVersion) throw new BusinessException("Otro usuario modificó el producto. Actualiza la lista.", 409);
            if (current[0].Stock != input.Stock) throw new BusinessException("El stock cambió. Usa Ajustar stock para registrar una reposición o corrección.", 409);
            id = current[0].Id;
            await using var command = Command("""
                UPDATE bodega_norte.products SET name=@name,category_id=@category,supplier_id=CAST(@supplier AS integer),
                unit=@unit,min_stock=@min,cost=@cost,sale_price=@price,version=version+1,updated_at=now() WHERE code=@code
                """, c, tx, args);
            if (supplier is null) command.Parameters["supplier"].NpgsqlDbType = NpgsqlDbType.Integer;
            await command.ExecuteNonQueryAsync(ct);
        }
        await Audit(isNew ? "create" : "update", "product", code, input, actor, c, tx, ct);
        var saved = (await Query(ProductsSql + " AND p.code=@code", c, tx, ct, MapProduct, ("code", code))).Single();
        await tx.CommitAsync(ct);
        return saved;
    }

    public async Task DeleteProductAsync(string code, Actor actor, CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        if (await Execute("UPDATE bodega_norte.products SET is_active=false,version=version+1,updated_at=now() WHERE code=@code AND is_active=true", c, tx, ct, ("code", code)) == 0)
            throw new BusinessException("El producto no existe.", 404);
        await Audit("delete", "product", code, new { }, actor, c, tx, ct);
        await tx.CommitAsync(ct);
    }

    public async Task<ProductDto> AdjustStockAsync(string code, StockInput input, Actor actor, CancellationToken ct)
    {
        BodegaService.ValidateNumber(input.Quantity);
        if (input.Quantity == 0 || string.IsNullOrWhiteSpace(input.Reason)) throw new BusinessException("Indica una cantidad distinta de cero y el motivo.");
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        var rows = await Query("SELECT product_id,stock,version FROM bodega_norte.products WHERE code=@code AND is_active=true FOR UPDATE",
            c, tx, ct, r => (Id: r.GetInt32(0), Stock: r.GetDecimal(1), Version: r.GetInt64(2)), ("code", code));
        if (rows.Count == 0) throw new BusinessException("El producto no existe.", 404);
        if (rows[0].Version != input.ExpectedVersion) throw new BusinessException("El stock cambió. Actualiza antes de ajustar.", 409);
        var balance = rows[0].Stock + input.Quantity;
        if (balance < 0 || balance > 9999999999.99m) throw new BusinessException("El ajuste dejaría un stock fuera del límite.");
        await Execute("UPDATE bodega_norte.products SET stock=@balance,version=version+1,updated_at=now() WHERE product_id=@id",
            c, tx, ct, ("balance", balance), ("id", rows[0].Id));
        await Movement(rows[0].Id, input.Quantity, balance, "adjustment", input.Reason.Trim(), actor, c, tx, ct);
        await Audit("adjust_stock", "product", code, input, actor, c, tx, ct);
        var saved = (await Query(ProductsSql + " AND p.code=@code", c, tx, ct, MapProduct, ("code", code))).Single();
        await tx.CommitAsync(ct);
        return saved;
    }

    private const string SalesSql = """
        SELECT s.sale_number,s.sold_at,s.customer,COALESCE(SUM(i.quantity),0),s.total,s.payment_method,
        s.status,s.actor_email,s.cancel_reason,
        COALESCE(jsonb_agg(jsonb_build_object('productId',p.code,'name',COALESCE(i.product_name,p.name),
          'quantity',i.quantity,'unitPrice',i.unit_price,'unitCost',i.unit_cost,'total',i.line_total)
          ORDER BY i.sale_item_id) FILTER(WHERE i.sale_item_id IS NOT NULL),'[]'::jsonb)::text,s.request_id
        FROM bodega_norte.sales s LEFT JOIN bodega_norte.sale_items i USING(sale_id)
        LEFT JOIN bodega_norte.products p USING(product_id)
        """;
    private static SaleDto MapSale(NpgsqlDataReader r) => new(r.GetString(0), r.GetFieldValue<DateTimeOffset>(1),
        r.GetString(2), r.GetDecimal(3), r.GetDecimal(4), r.GetString(5), r.GetString(6), r.GetString(7),
        r.IsDBNull(8) ? null : r.GetString(8), JsonSerializer.Deserialize<List<SaleLineDto>>(r.GetString(9), Json)!, r.IsDBNull(10) ? null : r.GetGuid(10));
    private async Task<SaleDto> ReadSale(string number, NpgsqlConnection c, NpgsqlTransaction? tx, CancellationToken ct) =>
        (await Query(SalesSql + " WHERE s.sale_number=@number GROUP BY s.sale_id", c, tx, ct, MapSale, ("number", number))).Single();
    public async Task<IReadOnlyList<SaleDto>> GetSalesAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        // Se devuelve el historial completo; los filtros y agregados no dependen de un LIMIT 100.
        return await Query(SalesSql + " GROUP BY s.sale_id ORDER BY s.sold_at DESC", c, null, ct, MapSale);
    }

    public async Task<SaleDto> CreateSaleAsync(SaleInput input, Actor actor, CancellationToken ct)
    {
        var items = input.Items.GroupBy(i => i.ProductId, StringComparer.Ordinal)
            .Select(g => new { Code = g.Key, Quantity = g.Sum(i => i.Quantity), ExpectedPrice = g.First().ExpectedPrice }).OrderBy(i => i.Code, StringComparer.Ordinal).ToList();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { input.PaymentMethod, input.Customer, items }, Json))));
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        // Serializa reintentos con la misma clave antes de comprobar o insertar la venta.
        await Scalar("SELECT pg_advisory_xact_lock(hashtextextended(@key,0))", c, tx, ct, ("key", input.RequestId.ToString()));
        var previous = await Query("SELECT sale_number,request_hash,created_by FROM bodega_norte.sales WHERE request_id=@key",
            c, tx, ct, r => (Number: r.GetString(0), Hash: r.GetString(1), Actor: r.GetGuid(2)), ("key", input.RequestId));
        if (previous.Count > 0)
        {
            if (previous[0].Hash != hash || previous[0].Actor != actor.Id) throw new BusinessException("La clave ya pertenece a otra operación.", 409);
            var existing = await ReadSale(previous[0].Number, c, tx, ct);
            await tx.CommitAsync(ct);
            return existing;
        }
        var lines = new List<(int Id, string Code, string Name, decimal Quantity, decimal Price, decimal Cost, decimal Balance)>();
        foreach (var item in items)
        {
            BodegaService.ValidateNumber(item.Quantity);
            var product = await Query("SELECT product_id,name,stock,sale_price,cost FROM bodega_norte.products WHERE code=@code AND is_active=true FOR UPDATE",
                c, tx, ct, r => (Id: r.GetInt32(0), Name: r.GetString(1), Stock: r.GetDecimal(2), Price: r.GetDecimal(3), Cost: r.GetDecimal(4)), ("code", item.Code));
            if (product.Count == 0) throw new BusinessException($"El producto {item.Code} no está disponible.");
            if (item.Quantity > product[0].Stock) throw new BusinessException($"Stock insuficiente para {product[0].Name}. Disponible: {product[0].Stock:0.##}.");
            if (item.ExpectedPrice is { } price && price != product[0].Price) throw new BusinessException($"El precio de {product[0].Name} cambió. Actualiza el carrito antes de cobrar.", 409);
            lines.Add((product[0].Id, item.Code, product[0].Name, item.Quantity, product[0].Price, product[0].Cost, product[0].Stock - item.Quantity));
        }
        var total = lines.Sum(l => decimal.Round(l.Quantity * l.Price, 2, MidpointRounding.AwayFromZero));
        BodegaService.ValidateNumber(total);
        var number = $"V-{Guid.NewGuid():N}";
        var id = Convert.ToInt32(await Scalar("""
            INSERT INTO bodega_norte.sales(sale_number,payment_method,total,request_id,request_hash,created_by,actor_email,customer)
            VALUES(@number,@method,@total,@key,@hash,@actor,@email,@customer) RETURNING sale_id
            """, c, tx, ct, ("number", number), ("method", input.PaymentMethod), ("total", total), ("key", input.RequestId),
            ("hash", hash), ("actor", actor.Id), ("email", actor.Email), ("customer", string.IsNullOrWhiteSpace(input.Customer) ? "Venta mostrador" : input.Customer.Trim())));
        foreach (var line in lines)
        {
            await Execute("""
                INSERT INTO bodega_norte.sale_items(sale_id,product_id,quantity,unit_price,product_name,unit_cost)
                VALUES(@sale,@id,@quantity,@price,@name,@cost)
                """, c, tx, ct, ("sale", id), ("id", line.Id), ("quantity", line.Quantity), ("price", line.Price), ("name", line.Name), ("cost", line.Cost));
            await Execute("UPDATE bodega_norte.products SET stock=@balance,version=version+1,updated_at=now() WHERE product_id=@id",
                c, tx, ct, ("balance", line.Balance), ("id", line.Id));
            await Movement(line.Id, -line.Quantity, line.Balance, "sale", number, actor, c, tx, ct);
        }
        await Audit("create", "sale", number, new { total, input.RequestId }, actor, c, tx, ct);
        var sale = await ReadSale(number, c, tx, ct);
        await tx.CommitAsync(ct);
        return sale;
    }

    public async Task<SaleDto> CancelSaleAsync(string number, string reason, Actor actor, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException("Indica el motivo de anulación.");
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        var saleRows = await Query("SELECT sale_id,status FROM bodega_norte.sales WHERE sale_number=@number FOR UPDATE", c, tx, ct,
            r => (Id: r.GetInt32(0), Status: r.GetString(1)), ("number", number));
        if (saleRows.Count == 0) throw new BusinessException("La venta no existe.", 404);
        if (saleRows[0].Status != "cancelled")
        {
            var lines = await Query("""
                SELECT p.product_id,SUM(i.quantity)::numeric,p.stock FROM bodega_norte.sale_items i
                JOIN bodega_norte.products p USING(product_id) WHERE i.sale_id=@id
                GROUP BY p.product_id ORDER BY p.code
                """, c, tx, ct, r => (Id: r.GetInt32(0), Quantity: r.GetDecimal(1)), ("id", saleRows[0].Id));
            foreach (var line in lines)
            {
                var balance = Convert.ToDecimal(await Scalar("""
                    UPDATE bodega_norte.products SET stock=stock+@quantity,version=version+1,updated_at=now()
                    WHERE product_id=@id RETURNING stock
                    """, c, tx, ct, ("quantity", line.Quantity), ("id", line.Id)), CultureInfo.InvariantCulture);
                await Movement(line.Id, line.Quantity, balance, "cancellation", reason.Trim(), actor, c, tx, ct);
            }
            await Execute("""
                UPDATE bodega_norte.sales SET status='cancelled',cancelled_at=now(),cancelled_by=@actor,cancel_reason=@reason
                WHERE sale_id=@id
                """, c, tx, ct, ("actor", actor.Id), ("reason", reason.Trim()), ("id", saleRows[0].Id));
            await Audit("cancel", "sale", number, new { reason }, actor, c, tx, ct);
        }
        var result = await ReadSale(number, c, tx, ct);
        await tx.CommitAsync(ct);
        return result;
    }

    public async Task<IReadOnlyList<SupplierDto>> GetSuppliersAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        return await Query("""
            SELECT s.supplier_id,s.name,COALESCE(s.contact_name,''),COALESCE(s.phone,''),
            COUNT(p.product_id) FILTER(WHERE p.is_active=true)::integer FROM bodega_norte.suppliers s
            LEFT JOIN bodega_norte.products p USING(supplier_id) WHERE s.is_active=true GROUP BY s.supplier_id ORDER BY s.name
            """, c, null, ct, r => new SupplierDto(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetInt32(4)));
    }
    public async Task SaveSupplierAsync(int? id, SupplierInput input, Actor actor, CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        if (id is null)
            id = Convert.ToInt32(await Scalar("""
                INSERT INTO bodega_norte.suppliers(name,contact_name,phone) VALUES(@name,@contact,@phone) RETURNING supplier_id
                """, c, tx, ct, ("name", input.Name.Trim()), ("contact", input.Contact?.Trim() ?? ""), ("phone", input.Phone?.Trim() ?? "")));
        else if (await Execute("""
            UPDATE bodega_norte.suppliers SET name=@name,contact_name=@contact,phone=@phone WHERE supplier_id=@id AND is_active=true
            """, c, tx, ct, ("name", input.Name.Trim()), ("contact", input.Contact?.Trim() ?? ""), ("phone", input.Phone?.Trim() ?? ""), ("id", id.Value)) == 0)
            throw new BusinessException("El proveedor no existe.", 404);
        await Audit("save", "supplier", id.ToString()!, input, actor, c, tx, ct);
        await tx.CommitAsync(ct);
    }
    public async Task DeleteSupplierAsync(int id, Actor actor, CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        await Scalar("SELECT supplier_id FROM bodega_norte.suppliers WHERE supplier_id=@id FOR UPDATE", c, tx, ct, ("id", id));
        var count = Convert.ToInt32(await Scalar("SELECT count(*) FROM bodega_norte.products WHERE supplier_id=@id AND is_active=true", c, tx, ct, ("id", id)));
        if (count != 0) throw new BusinessException("Reasigna sus productos antes de eliminar el proveedor.", 409);
        if (await Execute("UPDATE bodega_norte.suppliers SET is_active=false WHERE supplier_id=@id AND is_active=true", c, tx, ct, ("id", id)) == 0)
            throw new BusinessException("El proveedor no existe.", 404);
        await Audit("delete", "supplier", id.ToString(), new { }, actor, c, tx, ct);
        await tx.CommitAsync(ct);
    }

    public async Task<SettingsDto> GetSettingsAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        return (await Query("SELECT name,ruc,address,phone,stock_alerts FROM bodega_norte.settings WHERE id=1",
            c, null, ct, r => new SettingsDto(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetBoolean(4)))).Single();
    }
    public async Task SaveSettingsAsync(SettingsDto input, Actor actor, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(input.Ruc) && !Regex.IsMatch(input.Ruc, "^[0-9]{11}$")) throw new BusinessException("El RUC debe tener 11 dígitos o quedar vacío.");
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        await Execute("UPDATE bodega_norte.settings SET name=@name,ruc=@ruc,address=@address,phone=@phone,stock_alerts=@alerts WHERE id=1",
            c, tx, ct, ("name", input.Name.Trim()), ("ruc", input.Ruc ?? ""), ("address", input.Address ?? ""), ("phone", input.Phone ?? ""), ("alerts", input.StockAlerts));
        await Audit("update", "settings", "1", input, actor, c, tx, ct);
        await tx.CommitAsync(ct);
    }
    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        return await Query("SELECT user_id,email,role,is_active FROM bodega_norte.members ORDER BY email", c, null, ct,
            r => new MemberDto(r.GetGuid(0), r.GetString(1), r.GetString(2), r.GetBoolean(3)));
    }
    public async Task SaveMemberAsync(MemberInput input, Actor actor, CancellationToken ct)
    {
        if (input.Role is not ("admin" or "cashier")) throw new BusinessException("El rol debe ser administrador o cajero.");
        var email = input.Email.Trim().ToLowerInvariant();
        if (email.Equals(actor.Email, StringComparison.OrdinalIgnoreCase)) throw new BusinessException("No puedes cambiar tus propios permisos.");
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(ct);
        var id = await Scalar("SELECT id FROM auth.users WHERE lower(email)=@email AND email_confirmed_at IS NOT NULL", c, tx, ct, ("email", email));
        if (id is not Guid userId) throw new BusinessException("La persona debe crear primero su cuenta y confirmar el correo.", 404);
        await Execute("""
            INSERT INTO bodega_norte.members(user_id,email,role,is_active) VALUES(@id,@email,@role,@active)
            ON CONFLICT(user_id) DO UPDATE SET email=@email,role=@role,is_active=@active
            """, c, tx, ct, ("id", userId), ("email", email), ("role", input.Role), ("active", input.Active));
        await Audit("permissions", "member", userId.ToString(), new { email, input.Role, input.Active }, actor, c, tx, ct);
        await tx.CommitAsync(ct);
    }
    public async Task<IReadOnlyList<StockMovementDto>> GetMovementsAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        return await Query("""
            SELECT m.movement_id,p.code,p.name,m.quantity,m.balance,m.type,m.reason,m.actor_email,m.created_at
            FROM bodega_norte.stock_movements m JOIN bodega_norte.products p USING(product_id) ORDER BY m.movement_id DESC LIMIT 1000
            """, c, null, ct, r => new StockMovementDto(r.GetInt64(0), r.GetString(1), r.GetString(2), r.GetDecimal(3), r.GetDecimal(4),
            r.GetString(5), r.GetString(6), r.GetString(7), r.GetFieldValue<DateTimeOffset>(8)));
    }
    public async Task<IReadOnlyList<AuditDto>> GetAuditAsync(CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        return await Query("SELECT audit_id,action,entity,entity_id,actor_email,created_at FROM bodega_norte.audit_log ORDER BY audit_id DESC LIMIT 1000",
            c, null, ct, r => new AuditDto(r.GetInt64(0), r.GetString(1), r.GetString(2), r.GetString(3), r.GetString(4), r.GetFieldValue<DateTimeOffset>(5)));
    }
    public async Task<Dictionary<string, JsonElement>> BackupAsync(Actor actor, CancellationToken ct)
    {
        await using var c = await _source.OpenConnectionAsync(ct);
        await using var tx = await c.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, ct);
        var data = new Dictionary<string, JsonElement>();
        foreach (var table in new[] { "categories", "suppliers", "products", "sales", "sale_items", "settings", "members", "stock_movements", "audit_log" })
        {
            var value = await Scalar($"SELECT COALESCE(jsonb_agg(to_jsonb(t)),'[]'::jsonb)::text FROM bodega_norte.{table} t", c, tx, ct);
            data[table] = JsonSerializer.Deserialize<JsonElement>((string)value!);
        }
        await Audit("export_backup", "database", _schema, new { }, actor, c, tx, ct);
        await tx.CommitAsync(ct);
        return data;
    }
}
