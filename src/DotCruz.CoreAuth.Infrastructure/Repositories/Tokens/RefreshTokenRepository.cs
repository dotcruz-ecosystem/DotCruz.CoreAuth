using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Infrastructure.Data;
using DotCruz.CoreAuth.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DotCruz.CoreAuth.Infrastructure.Repositories.Tokens;

public class RefreshTokenRepository(CoreAuthDbContext context) 
    : BaseRepository<RefreshToken>(context), IRefreshTokenReadRepository, IRefreshTokenWriteRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token && rt.DeletedAt == null && (rt.User == null || rt.User.DeletedAt == null), cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow && rt.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }
}
