namespace DotCruz.CoreAuth.Domain.Interfaces.Security
{
    public interface ITenantProvider
    {
        public Guid? TenantId();
    }
}
