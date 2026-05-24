namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class SurveyParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleType { get; set; } = "GuestPlaceholder";
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
