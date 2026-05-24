using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace;

public interface IRequirementWorkspaceStore
{
    Task<RequirementWorkspace> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(RequirementWorkspace workspace, CancellationToken cancellationToken);
}
