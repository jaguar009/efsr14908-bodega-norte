using BodegaNorte.Api.Models;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BodegaNorte.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly BodegaService _service;

    public ProductsController(BodegaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) => Ok(await _service.GetProductsAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Post(ProductInput input, CancellationToken cancellationToken)
    {
        try { return Ok(await _service.SaveProductAsync(input, cancellationToken)); }
        catch (InvalidOperationException error) { return BadRequest(new { message = error.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        try { await _service.DeleteProductAsync(id, cancellationToken); return NoContent(); }
        catch (KeyNotFoundException error) { return NotFound(new { message = error.Message }); }
    }
}
