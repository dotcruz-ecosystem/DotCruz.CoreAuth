using DotCruz.CoreAuth.Application.Commands.Auth.SwitchWorkspace;

namespace CommonTestUtilities.Requests.Auth;

public class SwitchWorkspaceCommandBuilder
{
    public static SwitchWorkspaceCommand Build(Guid? targetTenantId = null, bool isEmptyTenantId = false)
    {
        return new SwitchWorkspaceCommand(
            isEmptyTenantId ? Guid.Empty : (targetTenantId ?? Guid.NewGuid())
        );
    }
}
