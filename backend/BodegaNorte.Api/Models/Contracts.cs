namespace BodegaNorte.Api.Models;

public sealed record ProductDto(
    string Id,
    string Name,
    string Category,
    decimal Stock,
    decimal MinStock,
    decimal Cost,
    decimal Price,
    string Supplier,
    string Unit,
    string Updated);

public sealed record SaleDto(
    string Id,
    string Time,
    string Customer,
    decimal Items,
    decimal Total,
    string Method);

public sealed record ProductInput(
    string Id,
    string Name,
    string Category,
    decimal Stock,
    decimal MinStock,
    decimal Cost,
    decimal Price,
    string Supplier,
    string Unit);

public sealed record SaleItemInput(string ProductId, decimal Quantity);

public sealed record SaleInput(string PaymentMethod, IReadOnlyList<SaleItemInput> Items);

public sealed record SaleResponse(SaleDto Sale, IReadOnlyList<ProductDto> Products);
