using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;

namespace DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;

public interface IWorkspaceServiceClient
{
    Task<WorkspaceSummaryDto?> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserWorkspaceDto>> GetUserWorkspacesAsync(Guid userId, CancellationToken cancellationToken);
}
