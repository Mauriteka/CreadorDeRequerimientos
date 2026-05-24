namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class SurveyTranscriptTurn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SpeakerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public bool Important { get; set; }
    public string SourceType { get; set; } = "Manual";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void Update(Guid speakerId, string text, string tag, bool important, string sourceType)
    {
        SpeakerId = speakerId;
        Text = text.Trim();
        Tag = tag.Trim();
        Important = important;
        SourceType = string.IsNullOrWhiteSpace(sourceType) ? "Manual" : sourceType.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
