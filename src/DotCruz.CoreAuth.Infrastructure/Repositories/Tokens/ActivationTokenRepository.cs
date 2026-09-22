using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Infrastructure.Data;
using DotCruz.CoreAuth.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DotCruz.CoreAuth.Infrastructure.Repositories.Tokens;

public class ActivationTokenRepository : BaseRepository<ActivationToken>, IActivationTokenReadRepository, IActivationTokenWriteRepository
{
    public ActivationTokenRepository(CoreAuthDbContext context) : base(context) { }

    public async Task<ActivationToken?> GetActiveByTokenAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => 
                t.Token == tokenHash && 
                t.UsedAt == null && 
                t.ExpiresAt > DateTime.UtcNow &&
                t.DeletedAt == null &&
                (t.User == null || t.User.DeletedAt == null), 
                cancellationToken
            );
    }
}
