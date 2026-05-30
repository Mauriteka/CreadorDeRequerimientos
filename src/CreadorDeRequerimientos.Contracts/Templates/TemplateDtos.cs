namespace CreadorDeRequerimientos.Contracts;

public sealed record TemplateSummaryResponse(
    Guid Id,
    string Name,
    string Description,
    string Scope,
    int InterviewSectionCount,
    int MinuteSectionCount,
    DateTimeOffset UpdatedAt);

public sealed record TemplateDetailResponse(
    Guid Id,
    string Name,
    string Description,
    string Scope,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TemplateInterviewSectionResponse> InterviewSections,
    IReadOnlyList<TemplateMinuteSectionResponse> MinuteSections);

public sealed record AppliedTemplateResponse(
    Guid SourceTemplateId,
    string SourceScope,
    string Name,
    string Description,
    DateTimeOffset CapturedAt,
    IReadOnlyList<TemplateInterviewSectionResponse> InterviewSections,
    IReadOnlyList<TemplateMinuteSectionResponse> MinuteSections);

public sealed record TemplateInterviewSectionResponse(
    string Title,
    string Prompt,
    IReadOnlyList<string> Questions);

public sealed record TemplateMinuteSectionResponse(
    string Title,
    string Prompt);

public sealed record CreateTemplateRequest(
    string Name,
    string Description,
    IReadOnlyList<TemplateInterviewSectionRequest> InterviewSections,
    IReadOnlyList<TemplateMinuteSectionRequest> MinuteSections);

public sealed record UpdateTemplateRequest(
    string Name,
    string Description,
    IReadOnlyList<TemplateInterviewSectionRequest> InterviewSections,
    IReadOnlyList<TemplateMinuteSectionRequest> MinuteSections);

public sealed record TemplateInterviewSectionRequest(string Title, string Prompt, IReadOnlyList<string> Questions);

public sealed record TemplateMinuteSectionRequest(string Title, string Prompt);

public sealed record ExportProjectTemplateResponse(Guid TemplateId, string Name, string Scope);
