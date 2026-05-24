namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class SurveyMention
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public bool Important { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
