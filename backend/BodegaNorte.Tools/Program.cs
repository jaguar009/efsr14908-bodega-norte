using BodegaNorte.Api.Data;
using BodegaNorte.Tools;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

if (args.Length == 0 || args[0] is "--help" or "help")
{
    Console.WriteLine("Bodega Norte Tools\nverify --report RUTA_JSON\nassistant --report RUTA_JSON\nrestore RESPALDO_JSON --schema bodega_recovery_NOMBRE");
    return;
}
var root = new DirectoryInfo(Directory.GetCurrentDirectory());
while (root is not null && !File.Exists(Path.Combine(root.FullName, "database", "schema.sql"))) root = root.Parent;
if (root is null) throw new InvalidOperationException("Ejecuta desde la carpeta del proyecto o uno de sus subdirectorios.");
var configuration = new ConfigurationBuilder()
    .AddUserSecrets("BodegaNorte-EFSR14908-VisualStudio")
    .AddEnvironmentVariables().Build();
var privateConnection = configuration.GetConnectionString("BodegaNorte");
if (string.IsNullOrWhiteSpace(privateConnection)) throw new InvalidOperationException("Configura la conexión privada mediante User Secrets.");
var connection = DatabaseConnection.Normalize(privateConnection);
try
{
    if (args[0] == "assistant")
    {
        var reportIndex=Array.IndexOf(args,"--report");
        var report=reportIndex>=0&&reportIndex+1<args.Length?args[reportIndex+1]:"output/validacion_ibm.json";
        await AssistantVerification.RunAsync(root.FullName,configuration,report);
    }
    else if (args[0] == "verify")
    {
        var reportIndex = Array.IndexOf(args, "--report");
        var report = reportIndex >= 0 && reportIndex + 1 < args.Length ? args[reportIndex + 1] : "output/validacion_final.json";
        await Verification.RunAsync(root.FullName, connection, report);
    }
    else if (args[0] == "restore" && args.Length == 4 && args[2] == "--schema")
    {
        var recovery = await Recovery.RestoreAsync(root.FullName, connection, args[1], args[3]);
        Console.WriteLine(JsonSerializer.Serialize(recovery));
    }
    else throw new InvalidOperationException("Usa --help para consultar los comandos disponibles.");
}
catch (Exception error)
{
    // Connection strings and authentication tokens must never enter exported logs.
    Console.Error.WriteLine(error is Npgsql.PostgresException pg ? $"Operación rechazada por PostgreSQL: {pg.SqlState}." : error.Message);
    Environment.ExitCode = 1;
}
