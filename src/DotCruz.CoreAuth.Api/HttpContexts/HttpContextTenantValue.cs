using DotCruz.CoreAuth.Domain.Interfaces.Security;

namespace DotCruz.CoreAuth.Api.HttpContexts;

public class HttpContextTenantValue(IHttpContextAccessor contextAccessor) : ITenantProvider
{
    private const string TENANT_ID_HEADER_KEY = "X-Tenant-Id";

    public Guid? TenantId()
    {
        var httpContext = contextAccessor.HttpContext;
        
        if (httpContext == null)
            return null;

        if (httpContext.Request.Headers.TryGetValue(TENANT_ID_HEADER_KEY, out var values) && Guid.TryParse(values.ToString(), out var tenantId))
            return tenantId;

        return null;
    }
}
