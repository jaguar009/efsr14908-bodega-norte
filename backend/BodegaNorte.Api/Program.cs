using BodegaNorte.Api.Data;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");

var connectionString = builder.Configuration.GetConnectionString("BodegaNorte");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Falta ConnectionStrings:BodegaNorte. Configúrala en User Secrets o mediante la variable de entorno ConnectionStrings__BodegaNorte; no la guardes en appsettings.json.");

builder.Services.AddSingleton(new BodegaRepository(connectionString));
builder.Services.AddScoped<BodegaService>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication("SupabaseBearer")
    .AddScheme<AuthenticationSchemeOptions, SupabaseBearerAuthenticationHandler>("SupabaseBearer", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy
    .WithOrigins("http://localhost:5173", "https://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("frontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapFallbackToController("Index", "Home");
app.Run();
