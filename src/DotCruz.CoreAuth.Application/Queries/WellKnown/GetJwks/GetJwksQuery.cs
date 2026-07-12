using MediatR;

namespace DotCruz.CoreAuth.Application.Queries.WellKnown.GetJwks;

public record GetJwksQuery : IRequest<JwksResponseDto?>;
