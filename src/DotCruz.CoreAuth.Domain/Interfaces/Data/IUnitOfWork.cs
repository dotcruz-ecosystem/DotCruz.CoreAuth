namespace DotCruz.CoreAuth.Domain.Interfaces.Data
{
    public interface IUnitOfWork
    {
        public Task CommitAsync(CancellationToken cancellationToken);
    }
}
