using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace CreadorDeRequerimientos.API.Auth;

public sealed class WorkspaceAuthService(IOptions<WorkspaceAuthOptions> options)
{
    private readonly WorkspaceAuthOptions authOptions = options.Value;

    public bool IsEnabled => !string.IsNullOrWhiteSpace(authOptions.Password);

    public string Username => string.IsNullOrWhiteSpace(authOptions.Username)
        ? "admin"
        : authOptions.Username.Trim();

    public bool ValidateCredentials(string? username, string? password)
    {
        if (!IsEnabled)
        {
            return true;
        }

        var expectedUser = Encoding.UTF8.GetBytes(Username);
        var providedUser = Encoding.UTF8.GetBytes((username ?? string.Empty).Trim());
        var expectedPassword = Encoding.UTF8.GetBytes(authOptions.Password);
        var providedPassword = Encoding.UTF8.GetBytes(password ?? string.Empty);

        return FixedTimeEquals(expectedUser, providedUser) && FixedTimeEquals(expectedPassword, providedPassword);
    }

    private static bool FixedTimeEquals(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(left, right);
    }
}
