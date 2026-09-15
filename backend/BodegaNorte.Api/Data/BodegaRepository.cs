using BodegaNorte.Api.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BodegaNorte.Api.Data;

public sealed class BodegaRepository
{
    private readonly string _connectionString;

    public BodegaRepository(string connectionString) => _connectionString = connectionString;

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT p.code, p.name, c.name, p.stock, p.min_stock, p.cost, p.sale_price,
                   COALESCE(s.name, 'Sin proveedor'), p.unit,
                   CONVERT(varchar(16), p.updated_at, 120)
            FROM products p
            INNER JOIN categories c ON c.category_id = p.category_id
            LEFT JOIN suppliers s ON s.supplier_id = p.supplier_id
            WHERE p.is_active = 1
            ORDER BY p.code;
            """;
        var products = new List<ProductDto>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new ProductDto(
                reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5),
                reader.GetDecimal(6), reader.GetString(7), reader.GetString(8), reader.GetString(9)));
        }
        return products;
    }

    public async Task<IReadOnlyList<SaleDto>> GetSalesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP (100) s.sale_number, CONVERT(varchar(5), s.sold_at, 108),
                   'Venta mostrador', COALESCE(SUM(i.quantity), 0), s.total, s.payment_method
            FROM sales s
            LEFT JOIN sale_items i ON i.sale_id = s.sale_id
            GROUP BY s.sale_id, s.sale_number, s.sold_at, s.total, s.payment_method
            ORDER BY s.sold_at DESC;
            """;
        var sales = new List<SaleDto>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sales.Add(new SaleDto(
                reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetDecimal(3), reader.GetDecimal(4), reader.GetString(5)));
        }
        return sales;
    }

    public async Task<ProductDto> SaveProductAsync(ProductInput input, CancellationToken cancellationToken = default)
    {
        ValidateProduct(input);
        var code = string.IsNullOrWhiteSpace(input.Id) ? $"P-{Random.Shared.Next(100, 999)}" : input.Id.Trim();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var categoryId = await GetOrCreateLookupAsync(connection, transaction, "categories", "category_id", input.Category.Trim(), cancellationToken);
            var supplierId = await GetOrCreateLookupAsync(connection, transaction, "suppliers", "supplier_id", input.Supplier.Trim(), cancellationToken);
            const string sql = """
                IF EXISTS (SELECT 1 FROM products WHERE code = @code)
                    UPDATE products SET name=@name, category_id=@categoryId, supplier_id=@supplierId,
                        unit=@unit, stock=@stock, min_stock=@minStock, cost=@cost, sale_price=@price,
                        is_active=1, updated_at=SYSUTCDATETIME()
                    WHERE code=@code;
                ELSE
                    INSERT INTO products (code, name, category_id, supplier_id, unit, stock, min_stock, cost, sale_price)
                    VALUES (@code, @name, @categoryId, @supplierId, @unit, @stock, @minStock, @cost, @price);
                """;
            await using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@name", input.Name.Trim());
            command.Parameters.AddWithValue("@categoryId", categoryId);
            command.Parameters.AddWithValue("@supplierId", supplierId);
            command.Parameters.AddWithValue("@unit", input.Unit.Trim());
            command.Parameters.Add("@stock", SqlDbType.Decimal).Value = input.Stock;
            command.Parameters.Add("@minStock", SqlDbType.Decimal).Value = input.MinStock;
            command.Parameters.Add("@cost", SqlDbType.Decimal).Value = input.Cost;
            command.Parameters.Add("@price", SqlDbType.Decimal).Value = input.Price;
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        var products = await GetProductsAsync(cancellationToken);
        return products.First(product => product.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public async Task DeleteProductAsync(string code, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        const string sql = "UPDATE products SET is_active = 0, updated_at = SYSUTCDATETIME() WHERE code = @code AND is_active = 1;";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@code", code);
        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0) throw new KeyNotFoundException("El producto no existe.");
    }

    public async Task<SaleResponse> CreateSaleAsync(SaleInput input, CancellationToken cancellationToken = default)
    {
        var validMethods = new[] { "Efectivo", "Yape", "Plin", "Tarjeta" };
        if (!validMethods.Contains(input.PaymentMethod, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("El método de pago no es válido.");
        if (input.Items is null || input.Items.Count == 0)
            throw new InvalidOperationException("La venta debe contener al menos un producto.");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            var lines = new List<(int ProductId, string Code, decimal Quantity, decimal UnitPrice)>();
            decimal total = 0;
            foreach (var item in input.Items)
            {
                if (item.Quantity <= 0) throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
                const string productSql = """
                    SELECT product_id, code, stock, sale_price
                    FROM products WITH (UPDLOCK, ROWLOCK)
                    WHERE code = @code AND is_active = 1;
                    """;
                await using var productCommand = new SqlCommand(productSql, connection, transaction);
                productCommand.Parameters.AddWithValue("@code", item.ProductId);
                await using var reader = await productCommand.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken)) throw new InvalidOperationException($"El producto {item.ProductId} no existe.");
                var productId = reader.GetInt32(0);
                var code = reader.GetString(1);
                var stock = reader.GetDecimal(2);
                var unitPrice = reader.GetDecimal(3);
                await reader.CloseAsync();
                if (item.Quantity > stock) throw new InvalidOperationException($"Stock insuficiente para {code}. Disponible: {stock:0.##}.");
                lines.Add((productId, code, item.Quantity, unitPrice));
                total += item.Quantity * unitPrice;
            }

            const string saleSql = """
                DECLARE @saleNumber nvarchar(30) = CONCAT('V-', FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmssfff'));
                INSERT INTO sales (sale_number, payment_method, total)
                OUTPUT INSERTED.sale_number, INSERTED.sold_at
                VALUES (@saleNumber, @paymentMethod, @total);
                """;
            string saleNumber;
            DateTime soldAt;
            await using (var saleCommand = new SqlCommand(saleSql, connection, transaction))
            {
                saleCommand.Parameters.AddWithValue("@paymentMethod", input.PaymentMethod);
                saleCommand.Parameters.Add("@total", SqlDbType.Decimal).Value = total;
                await using var saleReader = await saleCommand.ExecuteReaderAsync(cancellationToken);
                await saleReader.ReadAsync(cancellationToken);
                saleNumber = saleReader.GetString(0);
                soldAt = saleReader.GetDateTime(1);
            }

            const string saleIdSql = "SELECT sale_id FROM sales WHERE sale_number = @saleNumber;";
            int saleId;
            await using (var idCommand = new SqlCommand(saleIdSql, connection, transaction))
            {
                idCommand.Parameters.AddWithValue("@saleNumber", saleNumber);
                saleId = Convert.ToInt32(await idCommand.ExecuteScalarAsync(cancellationToken));
            }
            foreach (var line in lines)
            {
                const string itemSql = """
                    INSERT INTO sale_items (sale_id, product_id, quantity, unit_price)
                    VALUES (@saleId, @productId, @quantity, @unitPrice);
                    UPDATE products SET stock = stock - @quantity, updated_at = SYSUTCDATETIME()
                    WHERE product_id = @productId;
                    """;
                await using var itemCommand = new SqlCommand(itemSql, connection, transaction);
                itemCommand.Parameters.AddWithValue("@saleId", saleId);
                itemCommand.Parameters.AddWithValue("@productId", line.ProductId);
                itemCommand.Parameters.Add("@quantity", SqlDbType.Decimal).Value = line.Quantity;
                itemCommand.Parameters.Add("@unitPrice", SqlDbType.Decimal).Value = line.UnitPrice;
                await itemCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
            var products = await GetProductsAsync(cancellationToken);
            var sale = new SaleDto($"#{saleNumber}", soldAt.ToLocalTime().ToString("HH:mm"), "Venta mostrador", lines.Sum(line => line.Quantity), total, input.PaymentMethod);
            return new SaleResponse(sale, products);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static void ValidateProduct(ProductInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new InvalidOperationException("El nombre del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(input.Category)) throw new InvalidOperationException("La categoría es obligatoria.");
        if (input.Stock < 0 || input.MinStock < 0 || input.Cost < 0 || input.Price <= 0)
            throw new InvalidOperationException("Stock, mínimo y costo no pueden ser negativos; el precio debe ser mayor que cero.");
    }

    private static async Task<int> GetOrCreateLookupAsync(SqlConnection connection, SqlTransaction transaction, string table, string idColumn, string value, CancellationToken cancellationToken)
    {
        var allowed = table == "categories" ? ("category_id", "categories") : ("supplier_id", "suppliers");
        var select = $"SELECT {allowed.Item1} FROM {allowed.Item2} WHERE name = @name;";
        await using var selectCommand = new SqlCommand(select, connection, transaction);
        selectCommand.Parameters.AddWithValue("@name", value);
        var found = await selectCommand.ExecuteScalarAsync(cancellationToken);
        if (found is not null && found != DBNull.Value) return Convert.ToInt32(found);

        var insert = table == "categories"
            ? "INSERT INTO categories (name) VALUES (@name); SELECT CAST(SCOPE_IDENTITY() AS INT);"
            : "INSERT INTO suppliers (name) VALUES (@name); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        await using var insertCommand = new SqlCommand(insert, connection, transaction);
        insertCommand.Parameters.AddWithValue("@name", value);
        return Convert.ToInt32(await insertCommand.ExecuteScalarAsync(cancellationToken));
    }
}
