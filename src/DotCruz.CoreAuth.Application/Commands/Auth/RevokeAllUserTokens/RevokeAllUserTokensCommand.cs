using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.RevokeAllUserTokens;

public record RevokeAllUserTokensCommand() : IRequest;
