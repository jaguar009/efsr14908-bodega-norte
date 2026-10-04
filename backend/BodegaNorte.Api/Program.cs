using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;
using BodegaNorte.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port)) builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
var connection = builder.Configuration.GetConnectionString("BodegaNorte");
if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Configura ConnectionStrings:BodegaNorte en User Secrets o en ConnectionStrings__BodegaNorte.");
builder.Services.AddSingleton(new BodegaRepository(DatabaseConnection.Normalize(connection), builder.Configuration));
builder.Services.AddScoped<BodegaService>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddAuthentication("SupabaseBearer").AddScheme<AuthenticationSchemeOptions, SupabaseBearerAuthenticationHandler>("SupabaseBearer", _ => { });
builder.Services.AddAuthorization(options => options.AddPolicy("Staff", policy => policy.RequireRole("admin", "cashier")));
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ??
    new[] { "http://localhost:5173", "https://localhost:5173", "https://jaguar009.github.io" };
builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (BusinessException error)
    {
        context.Response.StatusCode = error.Status;
        await context.Response.WriteAsJsonAsync(new { message = error.Message });
    }
    catch (PostgresException error) when (error.SqlState == "23505")
    {
        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(new { message = "Ya existe un registro con esos datos." });
    }
    catch (Exception error) when (!context.RequestAborted.IsCancellationRequested)
    {
        app.Logger.LogError(error, "Falló la operación {Path}", context.Request.Path);
        context.Response.StatusCode = 503;
        await context.Response.WriteAsJsonAsync(new { message = "El servicio no está disponible. Actualiza o reintenta la misma operación." });
    }
});
app.UseCors("frontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapFallbackToController("Index", "Home");
app.Run();
