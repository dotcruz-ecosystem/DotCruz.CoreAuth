using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Infrastructure.Data;
using DotCruz.CoreAuth.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DotCruz.CoreAuth.Infrastructure.Repositories.Tokens
{
    public class PasswordResetTokenRepository : BaseRepository<PasswordResetToken>, IPasswordResetTokenReadRepository, IPasswordResetTokenWriteRepository
    {
        public PasswordResetTokenRepository(CoreAuthDbContext context) : base(context) { }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
        {
            return await _dbSet
                .IgnoreQueryFilters()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.DeletedAt == null && (t.User == null || t.User.DeletedAt == null), cancellationToken);
        }
    }
}
