namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class RequirementWorkspace
{
    public List<RequirementProject> Projects { get; set; } = [];
    public List<SurveyTemplate> SystemTemplates { get; set; } = [];
}
