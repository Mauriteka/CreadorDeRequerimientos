namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class TemplateMinuteSection
{
    public string Title { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;

    public TemplateMinuteSection Clone()
    {
        return new TemplateMinuteSection
        {
            Title = Title.Trim(),
            Prompt = Prompt.Trim()
        };
    }
}
