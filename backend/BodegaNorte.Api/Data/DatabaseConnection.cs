using Npgsql;
namespace BodegaNorte.Api.Data;
public static class DatabaseConnection
{
    public static string Normalize(string value)
    {
        NpgsqlConnectionStringBuilder builder;
        if (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(value);
            var credentials = uri.UserInfo.Split(':', 2);
            if (credentials.Length != 2) throw new InvalidOperationException("Falta la contraseña en la cadena PostgreSQL.");
            builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host, Port = uri.Port > 0 ? uri.Port : 5432,
                Database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/')),
                Username = Uri.UnescapeDataString(credentials[0]), Password = Uri.UnescapeDataString(credentials[1]),
                SslMode = SslMode.VerifyFull
            };
        }
        else builder = new NpgsqlConnectionStringBuilder(value);
        builder.MaxPoolSize = 10; builder.MinPoolSize = 0; builder.Timeout = 15; builder.CommandTimeout = 30;
        builder.IncludeErrorDetail = false;
        if (builder.SslMode == SslMode.Disable) throw new InvalidOperationException("La conexión a PostgreSQL debe usar SSL.");
        var host = builder.Host ?? "";
        if (host.EndsWith(".supabase.co", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".pooler.supabase.com", StringComparison.OrdinalIgnoreCase))
        {
            builder.SslMode = SslMode.VerifyFull;
            if (string.IsNullOrWhiteSpace(builder.RootCertificate))
                builder.RootCertificate = Path.Combine(AppContext.BaseDirectory, "certificates", "prod-ca-2021.crt");
        }
        return builder.ConnectionString;
    }
}
