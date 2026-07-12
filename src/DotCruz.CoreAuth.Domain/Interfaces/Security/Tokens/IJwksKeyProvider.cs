namespace DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;

public record JwkKey(
    string Kty,
    string Use,
    string Alg,
    string Kid,
    string N,
    string E
);

public interface IJwksKeyProvider
{
    Task<IEnumerable<JwkKey>?> GetPublicKeysAsync(CancellationToken cancellationToken = default);
}
