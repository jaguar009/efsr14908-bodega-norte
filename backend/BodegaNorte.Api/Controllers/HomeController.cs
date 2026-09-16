using Microsoft.AspNetCore.Mvc;

namespace BodegaNorte.Api.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
