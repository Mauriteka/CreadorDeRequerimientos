namespace CreadorDeRequerimientos.Contracts;

public sealed record ProjectSummaryResponse(
    Guid Id,
    string Name,
    string FeatureName,
    int SurveyCount,
    int RequirementCount,
    int ProjectTemplateCount,
    DateTimeOffset UpdatedAt);

public sealed record ProjectDetailResponse(
    Guid Id,
    string Name,
    string FeatureName,
    string Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<SurveyResponse> Surveys,
    IReadOnlyList<RequirementDocumentResponse> Requirements,
    IReadOnlyList<TemplateDetailResponse> ProjectTemplates);

public sealed record CreateProjectRequest(string Name, string FeatureName, string Notes);

public sealed record UpdateProjectRequest(string Name, string FeatureName, string Notes);
