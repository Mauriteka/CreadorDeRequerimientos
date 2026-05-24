namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class RequirementDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Stage { get; set; } = "toma";
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<Guid> SurveyIds { get; set; } = [];
    public List<Guid> RelatedRequirementIds { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public static RequirementDocument Create(
        string stage,
        string title,
        string summary,
        string content,
        IEnumerable<Guid>? surveyIds,
        IEnumerable<Guid>? relatedRequirementIds)
    {
        var document = new RequirementDocument();
        document.Update(stage, title, summary, content, surveyIds, relatedRequirementIds);
        document.CreatedAt = document.UpdatedAt;
        return document;
    }

    public void Update(
        string stage,
        string title,
        string summary,
        string content,
        IEnumerable<Guid>? surveyIds,
        IEnumerable<Guid>? relatedRequirementIds)
    {
        Stage = string.IsNullOrWhiteSpace(stage) ? "toma" : stage.Trim();
        Title = string.IsNullOrWhiteSpace(title) ? "Requerimiento sin titulo" : title.Trim();
        Summary = summary.Trim();
        Content = content.Trim();
        SurveyIds = surveyIds?.Distinct().ToList() ?? [];
        RelatedRequirementIds = relatedRequirementIds?.Distinct().ToList() ?? [];
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
