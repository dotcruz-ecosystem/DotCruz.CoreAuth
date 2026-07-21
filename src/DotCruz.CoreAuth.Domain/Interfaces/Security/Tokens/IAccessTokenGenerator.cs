using DotCruz.CoreAuth.Domain.Entities.Users;

namespace DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens
{
    public interface IAccessTokenGenerator
    {
        public string Generate(User user, string? tenantType = null, string? tenantPlan = null);
    }
}
