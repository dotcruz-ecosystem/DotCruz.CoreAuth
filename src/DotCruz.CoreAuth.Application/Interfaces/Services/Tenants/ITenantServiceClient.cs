
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;

namespace DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;

public interface ITenantServiceClient
{
    Task<TenantSummaryDto?> GetTenantSummaryAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserWorkspaceDto>> GetUserWorkspacesAsync(Guid userId, CancellationToken cancellationToken);
}
