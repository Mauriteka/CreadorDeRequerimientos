namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class SurveyTemplateSnapshot
{
    public Guid SourceTemplateId { get; set; }
    public string SourceScope { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CapturedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<TemplateInterviewSection> InterviewSections { get; set; } = [];
    public List<TemplateMinuteSection> MinuteSections { get; set; } = [];
}
