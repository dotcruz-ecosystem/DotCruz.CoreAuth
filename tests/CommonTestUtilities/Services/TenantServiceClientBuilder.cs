using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;
using Moq;

namespace CommonTestUtilities.Services;

public class TenantServiceClientBuilder
{
    private readonly Mock<ITenantServiceClient> _mock = new();

    public TenantServiceClientBuilder SetupGetTenantSummary(Guid tenantId, TenantSummaryDto? response)
    {
        _mock.Setup(x => x.GetTenantSummaryAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        return this;
    }

    public TenantServiceClientBuilder SetupGetUserWorkspaces(Guid userId, IReadOnlyCollection<UserWorkspaceDto> workspaces)
    {
        _mock.Setup(x => x.GetUserWorkspacesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspaces);
        return this;
    }

    public ITenantServiceClient Build() => _mock.Object;
}
