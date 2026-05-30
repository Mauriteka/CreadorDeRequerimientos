using CreadorDeRequerimientos.Contracts;

namespace CreadorDeRequerimientos.Mobile.Services;

public interface IProjectService
{
    Task<IReadOnlyList<ProjectSummaryResponse>> GetProjectsAsync(CancellationToken cancellationToken);
    Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken);
}
