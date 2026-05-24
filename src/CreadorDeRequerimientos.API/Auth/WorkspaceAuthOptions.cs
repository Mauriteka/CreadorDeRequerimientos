namespace CreadorDeRequerimientos.API.Auth;

public sealed class WorkspaceAuthOptions
{
    public const string SectionName = "Auth";

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = string.Empty;
}
