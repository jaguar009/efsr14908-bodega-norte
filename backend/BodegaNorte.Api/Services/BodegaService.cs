using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;
namespace BodegaNorte.Api.Services;
public sealed class BodegaService(BodegaRepository repository)
{
    public Task<bool> TestConnectionAsync(CancellationToken ct) => repository.TestConnectionAsync(ct);
    public Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken ct) => repository.GetProductsAsync(ct);
    public Task<IReadOnlyList<SaleDto>> GetSalesAsync(CancellationToken ct) => repository.GetSalesAsync(ct);
    public Task<ProductDto> SaveProductAsync(string? code, ProductInput input, Actor actor, CancellationToken ct)
    {
        if (input.Stock < 0 || input.MinStock < 0 || input.Cost < 0 || input.Price <= 0)
            throw new BusinessException("Stock, mínimo y costo no pueden ser negativos; el precio debe ser positivo.");
        foreach (var value in new[] { input.Stock, input.MinStock, input.Cost, input.Price }) ValidateNumber(value);
        if (code is not null && input.ExpectedVersion is null) throw new BusinessException("Falta la versión del producto.");
        return repository.SaveProductAsync(code, input, actor, ct);
    }
    public Task<SaleDto> CreateSaleAsync(SaleInput input, Actor actor, CancellationToken ct)
    {
        if (input.RequestId == Guid.Empty) throw new BusinessException("Falta el identificador único de la venta.");
        if (input.Items is null || input.Items.Count is < 1 or > 200 || input.Items.Any(i => i is null || string.IsNullOrWhiteSpace(i.ProductId) || i.Quantity <= 0))
            throw new BusinessException("La venta requiere de 1 a 200 líneas con código y cantidad positiva.");
        foreach (var item in input.Items) ValidateNumber(item.Quantity);
        if (!new[] { "Efectivo", "Yape", "Plin", "Tarjeta" }.Contains(input.PaymentMethod))
            throw new BusinessException("El método de pago no es válido.");
        return repository.CreateSaleAsync(input, actor, ct);
    }
    public static void ValidateNumber(decimal value)
    {
        if (Math.Abs(value) > 9999999999.99m || decimal.Round(value, 2) != value)
            throw new BusinessException("Usa valores de hasta dos decimales dentro del límite permitido.");
    }
}
