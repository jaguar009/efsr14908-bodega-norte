using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;

namespace BodegaNorte.Api.Services;

public sealed class BodegaService
{
    private readonly BodegaRepository _repository;

    public BodegaService(BodegaRepository repository) => _repository = repository;

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken) => _repository.TestConnectionAsync(cancellationToken);

    public Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken cancellationToken) => _repository.GetProductsAsync(cancellationToken);

    public Task<IReadOnlyList<SaleDto>> GetSalesAsync(CancellationToken cancellationToken) => _repository.GetSalesAsync(cancellationToken);

    public Task DeleteProductAsync(string id, CancellationToken cancellationToken) => _repository.DeleteProductAsync(id, cancellationToken);

    public Task<ProductDto> SaveProductAsync(ProductInput input, CancellationToken cancellationToken)
    {
        ValidateProduct(input);
        return _repository.SaveProductAsync(input, cancellationToken);
    }

    public Task<SaleResponse> CreateSaleAsync(SaleInput input, CancellationToken cancellationToken)
    {
        if (input.Items is null || input.Items.Count == 0)
            throw new InvalidOperationException("La venta debe contener al menos un producto.");
        if (input.Items.Any(item => item.Quantity <= 0))
            throw new InvalidOperationException("Las cantidades deben ser mayores que cero.");
        return _repository.CreateSaleAsync(input, cancellationToken);
    }

    private static void ValidateProduct(ProductInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new InvalidOperationException("El nombre del producto es obligatorio.");
        if (string.IsNullOrWhiteSpace(input.Category)) throw new InvalidOperationException("La categoría es obligatoria.");
        if (input.Stock < 0 || input.MinStock < 0 || input.Cost < 0 || input.Price <= 0)
            throw new InvalidOperationException("Stock, mínimo y costo no pueden ser negativos; el precio debe ser mayor que cero.");
    }
}
