using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BodegaNorte.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly BodegaService _service;

    public HealthController(BodegaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            await _service.TestConnectionAsync(cancellationToken);
            return Ok(new { status = "ok", database = "sql-server-local", architecture = "mvc-strict", layers = new[] { "View", "Controller", "Model" } });
        }
        catch (Exception error)
        {
            return Problem(title: "No se pudo conectar con SQL Server local.", detail: error.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
