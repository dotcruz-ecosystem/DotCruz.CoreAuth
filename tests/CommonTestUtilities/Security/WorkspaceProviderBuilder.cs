using DotCruz.Shared.Security.Context;
using Moq;

namespace CommonTestUtilities.Security;

public class WorkspaceProviderBuilder
{
    private readonly Mock<IWorkspaceProvider> _mock = new();

    public WorkspaceProviderBuilder WithWorkspaceId(Guid? workspaceId)
    {
        _mock.Setup(x => x.WorkspaceId).Returns(workspaceId);
        return this;
    }

    public static IWorkspaceProvider Build(Guid? workspaceId = null)
    {
        var mock = new Mock<IWorkspaceProvider>();
        mock.Setup(x => x.WorkspaceId).Returns(workspaceId);
        return mock.Object;
    }

    public IWorkspaceProvider Build() => _mock.Object;
}
