using DotCruz.CoreAuth.Domain.Entities.Users;

namespace DotCruz.CoreAuth.Domain.Interfaces.Security;

public interface ILoggedUser
{
    public Task<User> User();
}
