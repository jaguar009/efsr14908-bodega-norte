using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Npgsql;
using NpgsqlTypes;

namespace BodegaNorte.Tools;

public sealed record RecoveryResult(string Schema, Dictionary<string, int> Rows, int MembersNotRestored, string BackupSha256);
public static class Recovery
{
    private static readonly string[] Tables = ["categories", "suppliers", "products", "sales", "sale_items", "settings", "stock_movements", "audit_log"];
    public static async Task<RecoveryResult> RestoreAsync(string root, string connection, string backupPath, string schema)
    {
        if (!Regex.IsMatch(schema, "^bodega_recovery_[a-z0-9_]{1,40}$"))
            throw new InvalidOperationException("Usa un esquema nuevo con prefijo bodega_recovery_ y hasta 40 letras minúsculas dígitos o guiones bajos.");
        var info = new FileInfo(backupPath);
        if (!info.Exists || info.Length > 50_000_000) throw new InvalidOperationException("Selecciona un respaldo JSON de hasta 50 MB.");
        var bytes = await File.ReadAllBytesAsync(backupPath);
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(backupPath));
        var document = json.RootElement;
        if (!document.TryGetProperty("version", out var version) || !version.TryGetInt32(out var v) || v != 2 ||
            !document.TryGetProperty("project", out var project) || project.GetString() != "Bodega Norte" ||
            !document.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object ||
            (document.TryGetProperty("format", out var format) && format.GetString() != "postgresql-v2"))
            throw new InvalidOperationException("El archivo debe ser un respaldo PostgreSQL v2 de Bodega Norte.");
        foreach (var table in Tables)
            if (!data.TryGetProperty(table, out var rows) || rows.ValueKind != JsonValueKind.Array || rows.GetArrayLength() > 100_000)
                throw new InvalidOperationException($"El respaldo no contiene una tabla válida: {table}.");
        if (data.GetProperty("settings").GetArrayLength() != 1)
            throw new InvalidOperationException("El respaldo debe contener una configuración de bodega.");
        await using var c = new NpgsqlConnection(connection);
        await c.OpenAsync();
        await using var tx = await c.BeginTransactionAsync();
        await using (var locked = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtextextended(@schema,0))", c, tx))
        { locked.Parameters.AddWithValue("schema", schema); await locked.ExecuteScalarAsync(); }
        await using (var exists = new NpgsqlCommand("SELECT EXISTS(SELECT FROM pg_namespace WHERE nspname=@schema)", c, tx))
        {
            exists.Parameters.AddWithValue("schema", schema);
            if ((bool)(await exists.ExecuteScalarAsync())!) throw new InvalidOperationException("El esquema ya existe. La recuperación requiere un destino nuevo.");
        }
        foreach (var script in new[] { "schema.sql", "upgrade_operations.sql" })
        {
            var sql = (await File.ReadAllTextAsync(Path.Combine(root, "database", script))).Replace("bodega_norte", schema, StringComparison.Ordinal);
            await using var command = new NpgsqlCommand(sql, c, tx);
            await command.ExecuteNonQueryAsync();
        }
        await using (var emptyDefaults = new NpgsqlCommand($"DELETE FROM \"{schema}\".settings", c, tx)) await emptyDefaults.ExecuteNonQueryAsync();
        var counts = new Dictionary<string, int>();
        foreach (var table in Tables)
        {
            var columns = new List<string>();
            await using (var describe = new NpgsqlCommand("SELECT column_name FROM information_schema.columns WHERE table_schema=@schema AND table_name=@table AND is_generated='NEVER' ORDER BY ordinal_position", c, tx))
            {
                describe.Parameters.AddWithValue("schema", schema); describe.Parameters.AddWithValue("table", table);
                await using var reader = await describe.ExecuteReaderAsync();
                while (await reader.ReadAsync()) columns.Add('"' + reader.GetString(0).Replace("\"", "\"\"") + '"');
            }
            var names = string.Join(',', columns);
            await using var insert = new NpgsqlCommand($"INSERT INTO \"{schema}\".{table}({names}) OVERRIDING SYSTEM VALUE SELECT {names} FROM jsonb_populate_recordset(NULL::\"{schema}\".{table},@rows)", c, tx);
            insert.Parameters.AddWithValue("rows", NpgsqlDbType.Jsonb, data.GetProperty(table).GetRawText());
            counts[table] = await insert.ExecuteNonQueryAsync();
            // Imported identity values must not collide with the next generated identifier.
            await using var sequences = new NpgsqlCommand("SELECT column_name FROM information_schema.columns WHERE table_schema=@schema AND table_name=@table AND is_identity='YES'", c, tx);
            sequences.Parameters.AddWithValue("schema", schema); sequences.Parameters.AddWithValue("table", table);
            var identities = new List<string>();
            await using (var reader = await sequences.ExecuteReaderAsync()) while (await reader.ReadAsync()) identities.Add(reader.GetString(0));
            foreach (var identity in identities)
            {
                await using var reset = new NpgsqlCommand($"SELECT setval(pg_get_serial_sequence(@relation,@column),GREATEST(COALESCE(MAX(\"{identity}\"),0),1),COUNT(*)>0) FROM \"{schema}\".{table}", c, tx);
                reset.Parameters.AddWithValue("relation", $"{schema}.{table}"); reset.Parameters.AddWithValue("column", identity);
                await reset.ExecuteScalarAsync();
            }
        }
        var membersSkipped = data.TryGetProperty("members", out var members) && members.ValueKind == JsonValueKind.Array ? members.GetArrayLength() : 0;
        // Auth users and access grants are never recreated from a business-data backup.
        var checksum = Convert.ToHexString(SHA256.HashData(bytes));
        await using (var audit = new NpgsqlCommand($"INSERT INTO \"{schema}\".audit_log(action,entity,entity_id,actor_email,details) VALUES('restore_backup','database',@schema,'Herramienta local de recuperación',@details)", c, tx))
        {
            audit.Parameters.AddWithValue("schema", schema);
            audit.Parameters.AddWithValue("details", NpgsqlDbType.Jsonb, JsonSerializer.Serialize(new { checksum, membersSkipped }));
            await audit.ExecuteNonQueryAsync();
        }
        await tx.CommitAsync();
        return new(schema, counts, membersSkipped, checksum);
    }
}
