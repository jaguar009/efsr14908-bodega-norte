using BodegaNorte.Api.Models;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Globalization;

namespace BodegaNorte.Api.Data;

public sealed class BodegaRepository
{
    private readonly string _connectionString;

    public BodegaRepository(string connectionString) => _connectionString = connectionString;

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT 1", connection);
        await command.ExecuteScalarAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT p.code, p.name, c.name, p.stock, p.min_stock, p.cost, p.sale_price,
                   COALESCE(s.name, 'Sin proveedor'), p.unit, p.updated_at
            FROM bodega_norte.products p
            INNER JOIN bodega_norte.categories c ON c.category_id = p.category_id
            LEFT JOIN bodega_norte.suppliers s ON s.supplier_id = p.supplier_id
            WHERE p.is_active = TRUE
            ORDER BY p.code;
            """;
        var products = new List<ProductDto>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new ProductDto(
                reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetDecimal(3), reader.GetDecimal(4), reader.GetDecimal(5),
                reader.GetDecimal(6), reader.GetString(7), reader.GetString(8),
                reader.GetDateTime(9).ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)));
        }
        return products;
    }

    public async Task<IReadOnlyList<SaleDto>> GetSalesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT '#' || s.sale_number, to_char(s.sold_at AT TIME ZONE 'America/Lima', 'HH24:MI'),
                   'Venta mostrador', COALESCE(SUM(i.quantity), 0)::numeric, s.total, s.payment_method
            FROM bodega_norte.sales s
            LEFT JOIN bodega_norte.sale_items i ON i.sale_id = s.sale_id
            GROUP BY s.sale_id, s.sale_number, s.sold_at, s.total, s.payment_method
            ORDER BY s.sold_at DESC
            LIMIT 100;
            """;
        var sales = new List<SaleDto>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
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
        var code = string.IsNullOrWhiteSpace(input.Id) ? $"P-{Random.Shared.Next(100000, 1000000)}" : input.Id.Trim();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var categoryId = await GetOrCreateLookupAsync(connection, transaction, "categories", input.Category.Trim(), cancellationToken);
            int? supplierId = string.IsNullOrWhiteSpace(input.Supplier)
                ? null
                : await GetOrCreateLookupAsync(connection, transaction, "suppliers", input.Supplier.Trim(), cancellationToken);

            const string sql = """
                INSERT INTO bodega_norte.products
                    (code, name, category_id, supplier_id, unit, stock, min_stock, cost, sale_price)
                VALUES
                    (@code, @name, @category_id, @supplier_id, @unit, @stock, @min_stock, @cost, @price)
                ON CONFLICT (code) DO UPDATE SET
                    name = EXCLUDED.name,
                    category_id = EXCLUDED.category_id,
                    supplier_id = EXCLUDED.supplier_id,
                    unit = EXCLUDED.unit,
                    stock = EXCLUDED.stock,
                    min_stock = EXCLUDED.min_stock,
                    cost = EXCLUDED.cost,
                    sale_price = EXCLUDED.sale_price,
                    is_active = TRUE,
                    updated_at = now();
                """;
            await using var command = new NpgsqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("code", code);
            command.Parameters.AddWithValue("name", input.Name.Trim());
            command.Parameters.AddWithValue("category_id", NpgsqlDbType.Integer, categoryId);
            command.Parameters.Add(new NpgsqlParameter("supplier_id", NpgsqlDbType.Integer)
            {
                Value = supplierId is null ? DBNull.Value : supplierId.Value
            });
            command.Parameters.AddWithValue("unit", input.Unit.Trim());
            AddNumeric(command, "stock", input.Stock);
            AddNumeric(command, "min_stock", input.MinStock);
            AddNumeric(command, "cost", input.Cost);
            AddNumeric(command, "price", input.Price);
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }

        var products = await GetProductsAsync(cancellationToken);
        return products.First(product => product.Id.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public async Task DeleteProductAsync(string code, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        const string sql = """
            UPDATE bodega_norte.products
            SET is_active = FALSE, updated_at = now()
            WHERE code = @code AND is_active = TRUE;
            """;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("code", code);
        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0) throw new KeyNotFoundException("El producto no existe.");
    }

    public async Task<SaleResponse> CreateSaleAsync(SaleInput input, CancellationToken cancellationToken = default)
    {
        var validMethods = new[] { "Efectivo", "Yape", "Plin", "Tarjeta" };
        if (string.IsNullOrWhiteSpace(input.PaymentMethod) ||
            !validMethods.Contains(input.PaymentMethod.Trim(), StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("El método de pago no es válido.");
        if (input.Items is null || input.Items.Count == 0)
            throw new InvalidOperationException("La venta debe contener al menos un producto.");
        if (input.Items.Any(item => item is null || string.IsNullOrWhiteSpace(item.ProductId) || item.Quantity <= 0))
            throw new InvalidOperationException("Cada producto debe tener código y cantidad mayor que cero.");

        var requestedItems = input.Items
            .GroupBy(item => item.ProductId.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => (Code: group.Key, Quantity: group.Sum(item => item.Quantity)))
            .OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            var lines = new List<(int ProductId, string Code, decimal Quantity, decimal UnitPrice)>();
            decimal total = 0;
            foreach (var item in requestedItems)
            {
                const string productSql = """
                    SELECT product_id, code, stock, sale_price
                    FROM bodega_norte.products
                    WHERE code = @code AND is_active = TRUE
                    FOR UPDATE;
                    """;
                await using var productCommand = new NpgsqlCommand(productSql, connection, transaction);
                productCommand.Parameters.AddWithValue("code", item.Code);
                await using var reader = await productCommand.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    throw new InvalidOperationException($"El producto {item.Code} no existe.");
                var productId = reader.GetInt32(0);
                var code = reader.GetString(1);
                var stock = reader.GetDecimal(2);
                var unitPrice = reader.GetDecimal(3);
                await reader.CloseAsync();
                if (item.Quantity > stock)
                    throw new InvalidOperationException($"Stock insuficiente para {code}. Disponible: {stock:0.##}.");
                lines.Add((productId, code, item.Quantity, unitPrice));
                total += item.Quantity * unitPrice;
            }

            var saleNumber = $"V-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
            const string saleSql = """
                INSERT INTO bodega_norte.sales (sale_number, payment_method, total)
                VALUES (@sale_number, @payment_method, @total)
                RETURNING sale_id, sale_number, sold_at;
                """;
            int saleId;
            string savedSaleNumber;
            DateTime soldAt;
            await using (var saleCommand = new NpgsqlCommand(saleSql, connection, transaction))
            {
                saleCommand.Parameters.AddWithValue("sale_number", saleNumber);
                saleCommand.Parameters.AddWithValue("payment_method", input.PaymentMethod.Trim());
                AddNumeric(saleCommand, "total", total);
                await using var saleReader = await saleCommand.ExecuteReaderAsync(cancellationToken);
                await saleReader.ReadAsync(cancellationToken);
                saleId = saleReader.GetInt32(0);
                savedSaleNumber = saleReader.GetString(1);
                soldAt = saleReader.GetDateTime(2);
            }

            foreach (var line in lines)
            {
                const string itemSql = """
                    INSERT INTO bodega_norte.sale_items (sale_id, product_id, quantity, unit_price)
                    VALUES (@sale_id, @product_id, @quantity, @unit_price);
                    """;
                await using (var itemCommand = new NpgsqlCommand(itemSql, connection, transaction))
                {
                    itemCommand.Parameters.AddWithValue("sale_id", NpgsqlDbType.Integer, saleId);
                    itemCommand.Parameters.AddWithValue("product_id", NpgsqlDbType.Integer, line.ProductId);
                    AddNumeric(itemCommand, "quantity", line.Quantity);
                    AddNumeric(itemCommand, "unit_price", line.UnitPrice);
                    await itemCommand.ExecuteNonQueryAsync(cancellationToken);
                }

                const string updateStockSql = """
                    UPDATE bodega_norte.products
                    SET stock = stock - @quantity, updated_at = now()
                    WHERE product_id = @product_id AND stock >= @quantity;
                    """;
                await using var stockCommand = new NpgsqlCommand(updateStockSql, connection, transaction);
                AddNumeric(stockCommand, "quantity", line.Quantity);
                stockCommand.Parameters.AddWithValue("product_id", NpgsqlDbType.Integer, line.ProductId);
                if (await stockCommand.ExecuteNonQueryAsync(cancellationToken) != 1)
                    throw new InvalidOperationException($"Stock insuficiente para {line.Code}.");
            }

            await transaction.CommitAsync(cancellationToken);
            var products = await GetProductsAsync(cancellationToken);
            var sale = new SaleDto(
                $"#{savedSaleNumber}", soldAt.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture),
                "Venta mostrador", lines.Sum(line => line.Quantity), total, input.PaymentMethod.Trim());
            return new SaleResponse(sale, products);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static async Task<int> GetOrCreateLookupAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        string table,
        string value,
        CancellationToken cancellationToken)
    {
        var (idColumn, tableName) = table switch
        {
            "categories" => ("category_id", "categories"),
            "suppliers" => ("supplier_id", "suppliers"),
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        };
        var sql = $"""
            INSERT INTO bodega_norte.{tableName} (name)
            VALUES (@name)
            ON CONFLICT (name) DO UPDATE SET is_active = TRUE
            RETURNING {idColumn};
            """;
        await using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("name", value);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), CultureInfo.InvariantCulture);
    }

    private static void AddNumeric(NpgsqlCommand command, string name, decimal value)
    {
        command.Parameters.Add(name, NpgsqlDbType.Numeric).Value = value;
    }

    private static void ValidateProduct(ProductInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new InvalidOperationException("El nombre del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(input.Category)) throw new InvalidOperationException("La categoría es obligatoria.");
        if (string.IsNullOrWhiteSpace(input.Unit)) throw new InvalidOperationException("La unidad es obligatoria.");
        if (input.Stock < 0 || input.MinStock < 0 || input.Cost < 0 || input.Price <= 0)
            throw new InvalidOperationException("Stock, mínimo y costo no pueden ser negativos; el precio debe ser mayor que cero.");
    }
}
