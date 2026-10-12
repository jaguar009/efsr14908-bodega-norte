using System.ComponentModel.DataAnnotations;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BodegaNorte.Api.Controllers;
[ApiController, Authorize(Policy = "Staff"), Route("api/assistant")]
public sealed class AssistantController(WatsonAssistantService assistant) : BodegaController
{
    [HttpGet("status")]
    public IActionResult Status() => Ok(new { configured = assistant.Configured, provider = "watson", mode = "ayuda operativa sin acceso a registros" });

    [HttpPost("message"), EnableRateLimiting("assistant"), RequestSizeLimit(4096)]
    public async Task<IActionResult> Message(AssistantMessage input, CancellationToken ct) =>
        Ok(await assistant.MessageAsync(input.Message, Actor.Id, ct));
}
public sealed record AssistantMessage([Required, StringLength(500)] string Message);
