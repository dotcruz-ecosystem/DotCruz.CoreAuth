namespace DotCruz.CoreAuth.Domain.Entities.Base
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DeletedAt { get; protected set; }

        public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
        public void Delete() => DeletedAt = DateTimeOffset.UtcNow;
    }
}
