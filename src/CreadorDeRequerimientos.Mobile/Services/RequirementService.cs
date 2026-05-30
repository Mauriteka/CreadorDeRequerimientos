using CreadorDeRequerimientos.Contracts;

namespace CreadorDeRequerimientos.Mobile.Services;

public sealed class RequirementService(CreadorApiClient apiClient) : IRequirementService
{
    public Task<ProjectDetailResponse?> CreateDraftAsync(Guid projectId, CreateDraftRequirementRequest request, CancellationToken cancellationToken)
    {
        return apiClient.PostAsync<ProjectDetailResponse>($"api/projects/{projectId}/requirements/draft", request, cancellationToken);
    }
}
