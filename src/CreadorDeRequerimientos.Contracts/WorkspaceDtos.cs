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

public sealed record SurveyResponse(
    Guid Id,
    string Title,
    string Interviewee,
    string Objective,
    string OwnerEmail,
    string IntervieweeEmail,
    string ExtraEmails,
    string MinuteDraft,
    bool IsFinalized,
    DateTimeOffset? FinalizedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string SuggestedMinute,
    string ConversationCopy,
    AppliedTemplateResponse? AppliedTemplate,
    IReadOnlyList<SurveyParticipantResponse> Participants,
    IReadOnlyList<TranscriptTurnResponse> TranscriptTurns,
    IReadOnlyList<MentionResponse> Mentions);

public sealed record SurveyParticipantResponse(
    Guid Id,
    string DisplayName,
    string Email,
    string RoleType,
    int SortOrder,
    DateTimeOffset CreatedAt);

public sealed record TranscriptTurnResponse(
    Guid Id,
    Guid SpeakerId,
    string SpeakerName,
    string Text,
    string Tag,
    bool Important,
    string SourceType,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record MentionResponse(
    Guid Id,
    string Text,
    string Tag,
    bool Important,
    DateTimeOffset CreatedAt);

public sealed record RequirementDocumentResponse(
    Guid Id,
    string Stage,
    string Title,
    string Summary,
    string Content,
    IReadOnlyList<Guid> SurveyIds,
    IReadOnlyList<Guid> RelatedRequirementIds,
    IReadOnlyList<LinkedSurveySummaryResponse> LinkedSurveySummaries,
    IReadOnlyList<LinkedRequirementSummaryResponse> RelatedRequirementSummaries,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record LinkedSurveySummaryResponse(
    Guid Id,
    string Title,
    string Interviewee);

public sealed record LinkedRequirementSummaryResponse(
    Guid Id,
    string Title,
    string Stage);

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

public sealed record CreateProjectRequest(string Name, string FeatureName, string Notes);
public sealed record UpdateProjectRequest(string Name, string FeatureName, string Notes);
public sealed record CreateSurveyRequest(
    string Title,
    string Interviewee,
    string Objective,
    Guid? TemplateId,
    string? TemplateScope,
    string? OwnerEmail,
    string? IntervieweeEmail,
    string? ExtraEmails,
    string? MinuteDraft,
    bool IsFinalized);
public sealed record UpdateSurveyRequest(
    string Title,
    string Interviewee,
    string Objective,
    Guid? TemplateId,
    string? TemplateScope,
    string? OwnerEmail,
    string? IntervieweeEmail,
    string? ExtraEmails,
    string? MinuteDraft,
    bool IsFinalized);
public sealed record CreateTemplateRequest(string Name, string Description, IReadOnlyList<TemplateInterviewSectionRequest> InterviewSections, IReadOnlyList<TemplateMinuteSectionRequest> MinuteSections);
public sealed record UpdateTemplateRequest(string Name, string Description, IReadOnlyList<TemplateInterviewSectionRequest> InterviewSections, IReadOnlyList<TemplateMinuteSectionRequest> MinuteSections);
public sealed record TemplateInterviewSectionRequest(string Title, string Prompt, IReadOnlyList<string> Questions);
public sealed record TemplateMinuteSectionRequest(string Title, string Prompt);
public sealed record ExportProjectTemplateResponse(Guid TemplateId, string Name, string Scope);
public sealed record CreateSurveyParticipantRequest(string? DisplayName, string RoleType);
public sealed record RenameParticipantRequest(string DisplayName, string? Email);
public sealed record AddTranscriptTurnRequest(Guid SpeakerId, string Text, string Tag, bool Important, string SourceType);
public sealed record UpdateTranscriptTurnRequest(Guid SpeakerId, string Text, string Tag, bool Important, string SourceType);
public sealed record UpsertRequirementRequest(
    Guid? Id,
    string Stage,
    string Title,
    string Summary,
    string Content,
    IReadOnlyList<Guid> SurveyIds,
    IReadOnlyList<Guid> RelatedRequirementIds);

public sealed record CreateDraftRequirementRequest(string Stage, string Title, Guid? RequirementId);
public sealed record LoginRequest(string Username, string Password);
public sealed record AuthStatusResponse(bool Enabled, bool IsAuthenticated, string? Username);
