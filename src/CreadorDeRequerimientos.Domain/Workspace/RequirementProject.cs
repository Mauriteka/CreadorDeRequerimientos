namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class RequirementProject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<UserSurvey> Surveys { get; set; } = [];
    public List<RequirementDocument> Requirements { get; set; } = [];
    public List<SurveyTemplate> ProjectTemplates { get; set; } = [];

    public static RequirementProject Create(string name, string featureName, string notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El proyecto necesita un nombre.", nameof(name));
        }

        return new RequirementProject
        {
            Name = name.Trim(),
            FeatureName = featureName.Trim(),
            Notes = notes.Trim()
        };
    }

    public void Rename(string name, string featureName, string notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El proyecto necesita un nombre.", nameof(name));
        }

        Name = name.Trim();
        FeatureName = featureName.Trim();
        Notes = notes.Trim();
        Touch();
    }

    public UserSurvey AddSurvey(string title, string interviewee, string objective, SurveyTemplateSnapshot? appliedTemplate)
    {
        var survey = UserSurvey.Create(title, interviewee, objective, appliedTemplate);
        Surveys.Add(survey);
        Touch();
        return survey;
    }

    public RequirementDocument UpsertRequirement(
        Guid? id,
        string stage,
        string title,
        string summary,
        string content,
        IEnumerable<Guid>? surveyIds,
        IEnumerable<Guid>? relatedRequirementIds)
    {
        var requirement = id.HasValue
            ? Requirements.FirstOrDefault(item => item.Id == id.Value)
            : null;

        if (requirement is null)
        {
            requirement = RequirementDocument.Create(stage, title, summary, content, surveyIds, relatedRequirementIds);
            Requirements.Add(requirement);
        }
        else
        {
            requirement.Update(stage, title, summary, content, surveyIds, relatedRequirementIds);
        }

        Touch();
        return requirement;
    }

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
