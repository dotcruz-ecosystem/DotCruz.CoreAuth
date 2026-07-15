using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Infrastructure.Data;
using DotCruz.CoreAuth.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DotCruz.CoreAuth.Infrastructure.Repositories.Users;

public class UserRepository(CoreAuthDbContext context) 
    : BaseRepository<User>(context), IUserReadRepository, IUserWriteRepository
{
    public async Task<bool> ExistsActiveUserWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().IgnoreQueryFilters().AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<(IEnumerable<User> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> IsUserActive(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(u => 
                u.Id == userId &&
                u.Status == UserStatus.Active, 
                cancellationToken
            );
    }
}
