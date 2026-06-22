using DotCruz.CoreAuth.Domain.Entities.Base;

namespace DotCruz.CoreAuth.Domain.Interfaces.Repositories.Base
{
    public interface IBaseWriteRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdToUpdate(Guid id, CancellationToken cancellationToken);
        Task AddAsync(T entity, CancellationToken cancellationToken);
        Task AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    }
}
