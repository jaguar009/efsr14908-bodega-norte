using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
namespace BodegaNorte.Api.Controllers;
public sealed class HomeController(IWebHostEnvironment environment) : Controller
{
    public IActionResult Index()
    {
        var path = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), ".vite", "manifest.json");
        if (!System.IO.File.Exists(path)) return Content("Compila la interfaz con pnpm run build:visualstudio antes de iniciar MVC.", "text/plain");
        using var manifest = JsonDocument.Parse(System.IO.File.ReadAllText(path));
        var entry = manifest.RootElement.GetProperty("index.html");
        ViewData["Script"] = "/" + entry.GetProperty("file").GetString();
        ViewData["Styles"] = entry.TryGetProperty("css", out var css) ? css.EnumerateArray().Select(e => "/" + e.GetString()).ToArray() : Array.Empty<string>();
        return View();
    }
}
