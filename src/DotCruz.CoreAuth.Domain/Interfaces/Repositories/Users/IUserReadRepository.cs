using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Base;

namespace DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;

public interface IUserReadRepository : IBaseReadRepository<User>
{
    Task<bool> ExistsActiveUserWithEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<bool> IsUserActive(Guid userId, CancellationToken cancellationToken);
}
