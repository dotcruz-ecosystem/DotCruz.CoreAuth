using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;
using Moq;

namespace CommonTestUtilities.Services;

public class WorkspaceServiceClientBuilder
{
    private readonly Mock<IWorkspaceServiceClient> _mock = new();

    public WorkspaceServiceClientBuilder SetupGetWorkspaceSummary(Guid workspaceId, WorkspaceSummaryDto? response)
    {
        _mock.Setup(x => x.GetWorkspaceSummaryAsync(workspaceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        return this;
    }

    public WorkspaceServiceClientBuilder SetupGetUserWorkspaces(Guid userId, IReadOnlyCollection<UserWorkspaceDto> workspaces)
    {
        _mock.Setup(x => x.GetUserWorkspacesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspaces);
        return this;
    }

    public WorkspaceServiceClientBuilder SetupDefaultWorkspaceSummary()
    {
        _mock.Setup(x => x.GetWorkspaceSummaryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new WorkspaceSummaryDto(id, "Test Workspace", "test-workspace", "Active", "Standard", "Pro"));
        return this;
    }

    public IWorkspaceServiceClient Build() => _mock.Object;
}
