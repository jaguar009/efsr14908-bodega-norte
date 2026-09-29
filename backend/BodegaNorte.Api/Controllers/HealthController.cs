using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BodegaNorte.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly BodegaService _service;
    private readonly ILogger<HealthController> _logger;

    public HealthController(BodegaService service, ILogger<HealthController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            await _service.TestConnectionAsync(cancellationToken);
            return Ok(new { status = "ok", database = "supabase-postgresql", architecture = "mvc", layers = new[] { "View", "Controller", "Model" } });
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Falló la conexión del backend con Supabase PostgreSQL.");
            return Problem(title: "No se pudo conectar con Supabase PostgreSQL.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
