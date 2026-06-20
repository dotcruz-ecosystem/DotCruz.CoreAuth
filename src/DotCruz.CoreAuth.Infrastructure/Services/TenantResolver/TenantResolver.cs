using DotCruz.CoreAuth.Domain.Constants;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DotCruz.CoreAuth.Infrastructure.Services.TenantResolver;

public class TenantResolver : ITenantResolver
{
    private readonly IAuthTokenProvider _authTokenProvider;
    private readonly ITenantProvider _tenantProvider;
    private Guid? _resolvedTenantId;

    public TenantResolver(
        IAuthTokenProvider authTokenProvider,
        ITenantProvider tenantProvider
    )
    {
        _authTokenProvider = authTokenProvider;
        _tenantProvider = tenantProvider;
    }

    public Guid TenantId => _resolvedTenantId ??= SetTenantId();

    private Guid SetTenantId()
    {
        var token = _authTokenProvider.Value();
        if (string.IsNullOrWhiteSpace(token))
            return _tenantProvider.TenantId() ?? Guid.Empty;

        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(token))
            return _tenantProvider.TenantId() ?? Guid.Empty;

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

        var userRoleClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        var userTenantIdClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.TenantId)?.Value;

        if (userRoleClaim == null)
            return _tenantProvider.TenantId() ?? Guid.Empty;

        Guid userTenantIdGuid = Guid.Empty;
        
        if (Guid.TryParse(userTenantIdClaim, out var tenantId))
            userTenantIdGuid = tenantId;

        if (userRoleClaim != UserType.SuperAdmin.ToString() && userRoleClaim != UserType.InternalSupport.ToString())
            return userTenantIdGuid;

        return _tenantProvider.TenantId() ?? userTenantIdGuid;
    }
}
