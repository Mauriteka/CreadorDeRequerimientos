using System.Text.Json;
using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.Domain.Workspace;
using Microsoft.Data.Sqlite;

namespace CreadorDeRequerimientos.Infrastructure.Workspace;

public sealed class SqliteRequirementWorkspaceStore(string databasePath, string? legacyJsonPath) : IRequirementWorkspaceStore
{
    private const string WorkspaceKey = "default";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly SemaphoreSlim gate = new(1, 1);

    public async Task<RequirementWorkspace> LoadAsync(CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await EnsureSchemaAsync(connection, cancellationToken);

            var storedJson = await ReadWorkspaceJsonAsync(connection, cancellationToken);
            if (!string.IsNullOrWhiteSpace(storedJson))
            {
                return JsonSerializer.Deserialize<RequirementWorkspace>(storedJson, SerializerOptions)
                    ?? new RequirementWorkspace();
            }

            var importedWorkspace = await TryLoadLegacyJsonAsync(cancellationToken) ?? new RequirementWorkspace();
            await SaveWorkspaceAsync(connection, importedWorkspace, cancellationToken);
            return importedWorkspace;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(RequirementWorkspace workspace, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await EnsureSchemaAsync(connection, cancellationToken);
            await SaveWorkspaceAsync(connection, workspace, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static async Task EnsureSchemaAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS workspace_state (
                key TEXT PRIMARY KEY,
                json TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<string?> ReadWorkspaceJsonAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT json
            FROM workspace_state
            WHERE key = $key
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$key", WorkspaceKey);

        return await command.ExecuteScalarAsync(cancellationToken) as string;
    }

    private async Task<RequirementWorkspace?> TryLoadLegacyJsonAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(legacyJsonPath) || !File.Exists(legacyJsonPath))
        {
            return null;
        }

        await using var stream = File.OpenRead(legacyJsonPath);
        return await JsonSerializer.DeserializeAsync<RequirementWorkspace>(stream, SerializerOptions, cancellationToken);
    }

    private static async Task SaveWorkspaceAsync(
        SqliteConnection connection,
        RequirementWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(workspace, SerializerOptions);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO workspace_state (key, json, updated_at)
            VALUES ($key, $json, $updatedAt)
            ON CONFLICT(key) DO UPDATE SET
                json = excluded.json,
                updated_at = excluded.updated_at;
            """;
        command.Parameters.AddWithValue("$key", WorkspaceKey);
        command.Parameters.AddWithValue("$json", json);
        command.Parameters.AddWithValue("$updatedAt", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
