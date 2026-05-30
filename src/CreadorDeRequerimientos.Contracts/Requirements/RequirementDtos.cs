namespace CreadorDeRequerimientos.Contracts;

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

public sealed record LinkedRequirementSummaryResponse(
    Guid Id,
    string Title,
    string Stage);

public sealed record UpsertRequirementRequest(
    Guid? Id,
    string Stage,
    string Title,
    string Summary,
    string Content,
    IReadOnlyList<Guid> SurveyIds,
    IReadOnlyList<Guid> RelatedRequirementIds);

public sealed record CreateDraftRequirementRequest(string Stage, string Title, Guid? RequirementId);
