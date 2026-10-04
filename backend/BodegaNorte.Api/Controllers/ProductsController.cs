using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BodegaNorte.Api.Controllers;
[ApiController, Authorize(Policy = "Staff"), Route("api/products")]
public sealed class ProductsController(BodegaService service, BodegaRepository repository) : BodegaController
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct) => Ok(await service.GetProductsAsync(ct));
    [HttpPost, Authorize(Roles = "admin")]
    public async Task<IActionResult> Post(ProductInput input, CancellationToken ct) => Ok(await service.SaveProductAsync(null, input, Actor, ct));
    [HttpPut("{id}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Put(string id, ProductInput input, CancellationToken ct) => Ok(await service.SaveProductAsync(id, input, Actor, ct));
    [HttpPost("{id}/stock"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Stock(string id, StockInput input, CancellationToken ct) => Ok(await repository.AdjustStockAsync(id, input, Actor, ct));
    [HttpDelete("{id}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await repository.DeleteProductAsync(id, Actor, ct);
        return NoContent();
    }
}
