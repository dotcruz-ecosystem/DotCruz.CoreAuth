using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace DotCruz.CoreAuth.Infrastructure.Security.Tokens.Jwks;

public class JwksKeyProvider(IOptions<JwtTokenSettings> jwtTokenSettings) : IJwksKeyProvider
{
    private readonly JwtTokenSettings _jwtTokenSettings = jwtTokenSettings.Value;

    public Task<IEnumerable<JwkKey>?> GetPublicKeysAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_jwtTokenSettings.PrivateKeyPem) || 
            string.IsNullOrWhiteSpace(_jwtTokenSettings.Kid))
        {
            return Task.FromResult<IEnumerable<JwkKey>?>(null);
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(_jwtTokenSettings.PrivateKeyPem);

        var rsaSecurityKey = new RsaSecurityKey(rsa)
        {
            KeyId = _jwtTokenSettings.Kid
        };

        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaSecurityKey);

        var jwkKey = new JwkKey(
            Kty: jwk.Kty,
            Use: "sig",
            Alg: SecurityAlgorithms.RsaSha256,
            Kid: jwk.Kid,
            N: jwk.N,
            E: jwk.E
        );

        var list = new[] { jwkKey };
        return Task.FromResult<IEnumerable<JwkKey>?>(list);
    }
}
