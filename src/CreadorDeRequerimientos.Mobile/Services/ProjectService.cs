using CreadorDeRequerimientos.Contracts;

namespace CreadorDeRequerimientos.Mobile.Services;

public sealed class ProjectService(CreadorApiClient apiClient) : IProjectService
{
    public async Task<IReadOnlyList<ProjectSummaryResponse>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        return await apiClient.GetAsync<IReadOnlyList<ProjectSummaryResponse>>("api/projects", cancellationToken)
            ?? Array.Empty<ProjectSummaryResponse>();
    }

    public Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return apiClient.GetAsync<ProjectDetailResponse>($"api/projects/{projectId}", cancellationToken);
    }
}
