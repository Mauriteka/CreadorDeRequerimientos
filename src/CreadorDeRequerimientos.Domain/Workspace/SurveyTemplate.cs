namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class SurveyTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Scope { get; set; } = "system";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<TemplateInterviewSection> InterviewSections { get; set; } = [];
    public List<TemplateMinuteSection> MinuteSections { get; set; } = [];

    public static SurveyTemplate Create(
        string name,
        string description,
        string scope,
        IEnumerable<TemplateInterviewSection>? interviewSections,
        IEnumerable<TemplateMinuteSection>? minuteSections)
    {
        var template = new SurveyTemplate();
        template.Update(name, description, scope, interviewSections, minuteSections);
        template.CreatedAt = template.UpdatedAt;
        return template;
    }

    public void Update(
        string name,
        string description,
        string scope,
        IEnumerable<TemplateInterviewSection>? interviewSections,
        IEnumerable<TemplateMinuteSection>? minuteSections)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Plantilla sin nombre" : name.Trim();
        Description = description.Trim();
        Scope = string.IsNullOrWhiteSpace(scope) ? "system" : scope.Trim().ToLowerInvariant();
        InterviewSections = interviewSections?.Select(section => section.Clone()).ToList() ?? [];
        MinuteSections = minuteSections?.Select(section => section.Clone()).ToList() ?? [];
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public SurveyTemplateSnapshot ToSnapshot(Guid? sourceTemplateId = null)
    {
        return new SurveyTemplateSnapshot
        {
            SourceTemplateId = sourceTemplateId ?? Id,
            SourceScope = Scope,
            Name = Name,
            Description = Description,
            InterviewSections = InterviewSections.Select(section => section.Clone()).ToList(),
            MinuteSections = MinuteSections.Select(section => section.Clone()).ToList(),
            CapturedAt = DateTimeOffset.UtcNow
        };
    }

    public SurveyTemplate CloneAs(string scope)
    {
        return Create(Name, Description, scope, InterviewSections, MinuteSections);
    }
}
