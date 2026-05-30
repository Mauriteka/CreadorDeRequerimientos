using CreadorDeRequerimientos.Contracts;

namespace CreadorDeRequerimientos.Mobile.Services;

public sealed class SurveyService(CreadorApiClient apiClient) : ISurveyService
{
    public Task<ProjectDetailResponse?> CreateSurveyAsync(Guid projectId, CreateSurveyRequest request, CancellationToken cancellationToken)
    {
        return apiClient.PostAsync<ProjectDetailResponse>($"api/projects/{projectId}/surveys", request, cancellationToken);
    }

    public Task<ProjectDetailResponse?> AddTranscriptTurnAsync(Guid projectId, Guid surveyId, AddTranscriptTurnRequest request, CancellationToken cancellationToken)
    {
        return apiClient.PostAsync<ProjectDetailResponse>($"api/projects/{projectId}/surveys/{surveyId}/turns", request, cancellationToken);
    }
}
