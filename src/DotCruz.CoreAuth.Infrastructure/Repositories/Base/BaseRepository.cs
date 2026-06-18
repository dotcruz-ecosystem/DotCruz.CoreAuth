using DotCruz.CoreAuth.Domain.Entities.Base;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Base;
using DotCruz.CoreAuth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotCruz.CoreAuth.Infrastructure.Repositories.Base;

public class BaseRepository<T> : IBaseReadRepository<T>, IBaseWriteRepository<T> where T : BaseEntity
{
    protected readonly CoreAuthDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(CoreAuthDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().Where(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<T?> GetByIdToUpdate(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasAnyById(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.AsNoTracking().AnyAsync(e => e.Id == id, cancellationToken);
    }
}
