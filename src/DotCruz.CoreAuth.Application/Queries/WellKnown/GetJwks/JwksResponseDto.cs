namespace DotCruz.CoreAuth.Application.Queries.WellKnown.GetJwks;

public record JwkDto(
    string Kty,
    string Use,
    string Alg,
    string Kid,
    string N,
    string E
);

public record JwksResponseDto(
    IEnumerable<JwkDto> Keys
);
