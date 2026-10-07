using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.SwitchWorkspace;

public sealed record SwitchWorkspaceCommand(Guid TargetWorkspaceId) : IRequest<ResponseLoginDto>;
