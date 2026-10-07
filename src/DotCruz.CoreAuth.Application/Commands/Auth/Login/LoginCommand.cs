using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.Login
{
    public record LoginCommand(
        string Email, 
        string Password, 
        Guid? PreferredWorkspaceId = null
    ) : IRequest<ResponseLoginDto>;
}
