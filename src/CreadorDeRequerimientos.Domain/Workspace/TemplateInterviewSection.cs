namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class TemplateInterviewSection
{
    public string Title { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public List<string> Questions { get; set; } = [];

    public TemplateInterviewSection Clone()
    {
        return new TemplateInterviewSection
        {
            Title = Title.Trim(),
            Prompt = Prompt.Trim(),
            Questions = Questions.Select(question => question.Trim()).Where(question => question.Length > 0).ToList()
        };
    }
}
