using DotCruz.CoreAuth.Domain.Entities.Users;

namespace DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens
{
    public interface IAccessTokenGenerator
    {
        string Generate(
            User user, 
            string? tenantType = null, 
            string? tenantPlan = null, 
            Guid? activeTenantId = null, 
            string? workspaceRole = null);
    }
}
