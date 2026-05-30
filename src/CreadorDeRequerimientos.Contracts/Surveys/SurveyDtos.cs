namespace CreadorDeRequerimientos.Contracts;

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

public sealed record LinkedSurveySummaryResponse(
    Guid Id,
    string Title,
    string Interviewee);

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

public sealed record CreateSurveyParticipantRequest(string? DisplayName, string RoleType);

public sealed record RenameParticipantRequest(string DisplayName, string? Email);

public sealed record AddTranscriptTurnRequest(Guid SpeakerId, string Text, string Tag, bool Important, string SourceType);

public sealed record UpdateTranscriptTurnRequest(Guid SpeakerId, string Text, string Tag, bool Important, string SourceType);
