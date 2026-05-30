namespace CreadorDeRequerimientos.Contracts;

public sealed record LoginRequest(string Username, string Password);

public sealed record AuthStatusResponse(bool Enabled, bool IsAuthenticated, string? Username);
