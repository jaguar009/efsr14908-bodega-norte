using BodegaNorte.Api.Models;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BodegaNorte.Api.Controllers;

[ApiController]
[Route("api/sales")]
public sealed class SalesController : ControllerBase
{
    private readonly BodegaService _service;

    public SalesController(BodegaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) => Ok(await _service.GetSalesAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Post(SaleInput input, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.CreateSaleAsync(input, cancellationToken)); }
        catch (InvalidOperationException error) { return BadRequest(new { message = error.Message }); }
    }
}
