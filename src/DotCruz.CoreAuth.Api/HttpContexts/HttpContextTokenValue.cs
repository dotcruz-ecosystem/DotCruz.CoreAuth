using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;

namespace DotCruz.CoreAuth.Api.HttpContexts;

public class HttpContextTokenValue(IHttpContextAccessor contextAccessor) : IAuthTokenProvider
{
    public string Value()
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext == null)
            return string.Empty;

        var authorization = httpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return authorization["Bearer ".Length..].Trim();
    }
}
