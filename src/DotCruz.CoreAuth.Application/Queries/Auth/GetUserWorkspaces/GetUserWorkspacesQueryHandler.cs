using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using MediatR;

namespace DotCruz.CoreAuth.Application.Queries.Auth.GetUserWorkspaces;

public class GetUserWorkspacesQueryHandler : IRequestHandler<GetUserWorkspacesQuery, IReadOnlyCollection<UserWorkspaceDto>>
{
    private readonly ILoggedUser _loggedUser;
    private readonly IWorkspaceServiceClient _workspaceServiceClient;

    public GetUserWorkspacesQueryHandler(
        ILoggedUser loggedUser,
        IWorkspaceServiceClient workspaceServiceClient)
    {
        _loggedUser = loggedUser;
        _workspaceServiceClient = workspaceServiceClient;
    }

    public async Task<IReadOnlyCollection<UserWorkspaceDto>> Handle(GetUserWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var user = await _loggedUser.User(cancellationToken);
        return await _workspaceServiceClient.GetUserWorkspacesAsync(user.Id, cancellationToken);
    }
}
