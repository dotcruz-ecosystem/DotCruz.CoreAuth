using DotCruz.CoreAuth.Application.Commands.Auth.SwitchWorkspace;

namespace CommonTestUtilities.Requests.Auth;

public class SwitchWorkspaceCommandBuilder
{
    public static SwitchWorkspaceCommand Build(Guid? targetWorkspaceId = null, bool isEmptyWorkspaceId = false)
    {
        return new SwitchWorkspaceCommand(
            isEmptyWorkspaceId ? Guid.Empty : (targetWorkspaceId ?? Guid.NewGuid())
        );
    }
}
