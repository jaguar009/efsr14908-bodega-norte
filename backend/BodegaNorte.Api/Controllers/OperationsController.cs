using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BodegaNorte.Api.Controllers;
[ApiController, Authorize(Policy = "Staff"), Route("api")]
public sealed class OperationsController(BodegaRepository repository) : BodegaController
{
    [HttpGet("suppliers")] public async Task<IActionResult> Suppliers(CancellationToken ct) => Ok(await repository.GetSuppliersAsync(ct));
    [HttpPost("suppliers"), Authorize(Roles = "admin")]
    public async Task<IActionResult> AddSupplier(SupplierInput input, CancellationToken ct) { await repository.SaveSupplierAsync(null, input, Actor, ct); return NoContent(); }
    [HttpPut("suppliers/{id:int}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateSupplier(int id, SupplierInput input, CancellationToken ct) { await repository.SaveSupplierAsync(id, input, Actor, ct); return NoContent(); }
    [HttpDelete("suppliers/{id:int}"), Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteSupplier(int id, CancellationToken ct) { await repository.DeleteSupplierAsync(id, Actor, ct); return NoContent(); }
    [HttpGet("settings")] public async Task<IActionResult> Settings(CancellationToken ct) => Ok(await repository.GetSettingsAsync(ct));
    [HttpPut("settings"), Authorize(Roles = "admin")]
    public async Task<IActionResult> SaveSettings(SettingsDto input, CancellationToken ct) { await repository.SaveSettingsAsync(input, Actor, ct); return NoContent(); }
    [HttpGet("movements")] public async Task<IActionResult> Movements(CancellationToken ct) => Ok(await repository.GetMovementsAsync(ct));
    [HttpGet("audit"), Authorize(Roles = "admin")] public async Task<IActionResult> Audit(CancellationToken ct) => Ok(await repository.GetAuditAsync(ct));
    [HttpGet("members"), Authorize(Roles = "admin")] public async Task<IActionResult> Members(CancellationToken ct) => Ok(await repository.GetMembersAsync(ct));
    [HttpPut("members"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Member(MemberInput input, CancellationToken ct) { await repository.SaveMemberAsync(input, Actor, ct); return NoContent(); }
    [HttpGet("backup"), Authorize(Roles = "admin")]
    public async Task<IActionResult> Backup(CancellationToken ct) =>
        Ok(new { version = 2, exportedAt = DateTimeOffset.UtcNow, project = "Bodega Norte", data = await repository.BackupAsync(Actor, ct) });
}
[ApiController, Authorize, Route("api/me")]
public sealed class ProfileController : BodegaController
{
    [HttpGet] public IActionResult Get() => Ok(new { userId = Actor.Id, email = Actor.Email, role = Actor.Role });
}
