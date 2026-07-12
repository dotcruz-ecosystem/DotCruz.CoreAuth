using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;

namespace DotCruz.CoreAuth.Application.Queries.WellKnown.GetJwks;

public class GetJwksQueryHandler(IJwksKeyProvider jwksKeyProvider) 
    : IRequestHandler<GetJwksQuery, JwksResponseDto?>
{
    public async Task<JwksResponseDto?> Handle(GetJwksQuery request, CancellationToken cancellationToken)
    {
        var keys = await jwksKeyProvider.GetPublicKeysAsync(cancellationToken)
            ?? throw new NotFoundException(ResourceMessagesException.JWKS_NOT_FOUND);

        var jwkDtos = keys.Select(key => new JwkDto(
            Kty: key.Kty,
            Use: key.Use,
            Alg: key.Alg,
            Kid: key.Kid,
            N: key.N,
            E: key.E
        ));

        return new JwksResponseDto(jwkDtos);
    }
}
