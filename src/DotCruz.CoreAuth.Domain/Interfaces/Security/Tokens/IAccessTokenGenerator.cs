using DotCruz.CoreAuth.Domain.Entities.Users;

namespace DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens
{
    public interface IAccessTokenGenerator
    {
        string Generate(
            User user, 
            string? workspaceType = null, 
            string? workspacePlan = null, 
            Guid? activeWorkspaceId = null, 
            string? workspaceRole = null);
    }
}
