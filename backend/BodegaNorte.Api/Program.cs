using BodegaNorte.Api.Data;
using BodegaNorte.Api.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BodegaNorte")
    ?? throw new InvalidOperationException("No existe la cadena de conexión BodegaNorte.");

builder.Services.AddSingleton(new BodegaRepository(connectionString));
builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy
    .WithOrigins("http://localhost:5173", "https://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("frontend");
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", async (BodegaRepository repository, CancellationToken cancellationToken) =>
{
    try
    {
        await repository.TestConnectionAsync(cancellationToken);
        return Results.Ok(new { status = "ok", database = "sql-server-local" });
    }
    catch (Exception error)
    {
        return Results.Problem(title: "No se pudo conectar con SQL Server local.", detail: error.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/api/products", async (BodegaRepository repository, CancellationToken cancellationToken) =>
    Results.Ok(await repository.GetProductsAsync(cancellationToken)));

app.MapGet("/api/sales", async (BodegaRepository repository, CancellationToken cancellationToken) =>
    Results.Ok(await repository.GetSalesAsync(cancellationToken)));

app.MapPost("/api/products", async (ProductInput input, BodegaRepository repository, CancellationToken cancellationToken) =>
{
    try { return Results.Ok(await repository.SaveProductAsync(input, cancellationToken)); }
    catch (InvalidOperationException error) { return Results.BadRequest(new { message = error.Message }); }
});

app.MapDelete("/api/products/{id}", async (string id, BodegaRepository repository, CancellationToken cancellationToken) =>
{
    try { await repository.DeleteProductAsync(id, cancellationToken); return Results.NoContent(); }
    catch (KeyNotFoundException error) { return Results.NotFound(new { message = error.Message }); }
});

app.MapPost("/api/sales", async (SaleInput input, BodegaRepository repository, CancellationToken cancellationToken) =>
{
    try { return Results.Ok(await repository.CreateSaleAsync(input, cancellationToken)); }
    catch (InvalidOperationException error) { return Results.BadRequest(new { message = error.Message }); }
});

app.MapFallbackToFile("index.html");
app.Run();
