using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using MediatR;

namespace DotCruz.CoreAuth.Application.Queries.Auth.GetUserWorkspaces;

public class GetUserWorkspacesQueryHandler : IRequestHandler<GetUserWorkspacesQuery, IReadOnlyCollection<UserWorkspaceDto>>
{
    private readonly ILoggedUser _loggedUser;
    private readonly ITenantServiceClient _tenantServiceClient;

    public GetUserWorkspacesQueryHandler(
        ILoggedUser loggedUser,
        ITenantServiceClient tenantServiceClient)
    {
        _loggedUser = loggedUser;
        _tenantServiceClient = tenantServiceClient;
    }

    public async Task<IReadOnlyCollection<UserWorkspaceDto>> Handle(GetUserWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var user = await _loggedUser.User(cancellationToken);
        return await _tenantServiceClient.GetUserWorkspacesAsync(user.Id, cancellationToken);
    }
}
