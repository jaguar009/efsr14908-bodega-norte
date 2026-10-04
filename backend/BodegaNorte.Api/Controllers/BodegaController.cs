using System.Security.Claims;
using BodegaNorte.Api.Models;
using Microsoft.AspNetCore.Mvc;
namespace BodegaNorte.Api.Controllers;
public abstract class BodegaController : ControllerBase
{
    protected Actor Actor => new(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
        User.FindFirstValue(ClaimTypes.Email) ?? "", User.FindFirstValue(ClaimTypes.Role) ?? "pending");
}
