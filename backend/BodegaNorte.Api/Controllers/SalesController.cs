using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BodegaNorte.Api.Controllers;
[ApiController, Authorize(Policy = "Staff"), Route("api/sales")]
public sealed class SalesController(BodegaService service, BodegaRepository repository) : BodegaController
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct) => Ok(await service.GetSalesAsync(ct));
    [HttpPost] public async Task<IActionResult> Post(SaleInput input, CancellationToken ct) => Ok(await service.CreateSaleAsync(input, Actor, ct));
    [HttpPost("{id}/cancel"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Cancel(string id, CancelSaleInput input, CancellationToken ct) =>
        Ok(await repository.CancelSaleAsync(id, input.Reason, Actor, ct));
}
