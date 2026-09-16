using BodegaNorte.Api.Data;
using BodegaNorte.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BodegaNorte")
    ?? throw new InvalidOperationException("No existe la cadena de conexión BodegaNorte.");

builder.Services.AddSingleton(new BodegaRepository(connectionString));
builder.Services.AddScoped<BodegaService>();
builder.Services.AddControllersWithViews();
builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy
    .WithOrigins("http://localhost:5173", "https://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("frontend");
app.UseStaticFiles();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapFallbackToController("Index", "Home");
app.Run();
