namespace DotCruz.CoreAuth.Domain.Interfaces.Security;

public interface ITenantResolver
{
    public Guid TenantId { get; }
}
