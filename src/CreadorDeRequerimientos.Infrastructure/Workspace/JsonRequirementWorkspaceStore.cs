using System.Text.Json;
using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.Infrastructure.Workspace;

public sealed class JsonRequirementWorkspaceStore(string filePath) : IRequirementWorkspaceStore
{
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
            if (!File.Exists(filePath))
            {
                return new RequirementWorkspace();
            }

            await using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<RequirementWorkspace>(stream, SerializerOptions, cancellationToken)
                ?? new RequirementWorkspace();
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
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, workspace, SerializerOptions, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }
}
