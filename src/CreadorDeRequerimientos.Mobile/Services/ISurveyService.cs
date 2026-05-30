using CreadorDeRequerimientos.Contracts;

namespace CreadorDeRequerimientos.Mobile.Services;

public interface ISurveyService
{
    Task<ProjectDetailResponse?> CreateSurveyAsync(Guid projectId, CreateSurveyRequest request, CancellationToken cancellationToken);
    Task<ProjectDetailResponse?> AddTranscriptTurnAsync(Guid projectId, Guid surveyId, AddTranscriptTurnRequest request, CancellationToken cancellationToken);
}
