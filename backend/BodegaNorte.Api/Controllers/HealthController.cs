using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Mvc;
namespace BodegaNorte.Api.Controllers;
[ApiController, Route("api/health")]
public sealed class HealthController(BodegaService service) : ControllerBase
{
    [HttpGet] public IActionResult Get() => Ok(new { status = "ok", service = "Bodega Norte MVC" });
    [HttpGet("ready")] public async Task<IActionResult> Ready(CancellationToken ct) =>
        Ok(new { status = await service.TestConnectionAsync(ct) ? "ready" : "unavailable" });
}
