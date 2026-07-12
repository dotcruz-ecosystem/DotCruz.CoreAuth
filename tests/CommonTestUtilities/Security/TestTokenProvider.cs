using System.Security.Cryptography;
using System.Text;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;

namespace CommonTestUtilities.Security;

public class TestTokenProvider : ITokenProvider
{
    public string Value()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public string Hash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
