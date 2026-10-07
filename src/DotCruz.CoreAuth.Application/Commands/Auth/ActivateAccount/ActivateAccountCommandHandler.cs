using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotCruz.CoreAuth.Application.Commands.Auth.ActivateAccount;

public class ActivateAccountCommandHandler : IRequestHandler<ActivateAccountCommand, ResponseLoginDto>
{
    private readonly IActivationTokenReadRepository _activationTokenReadRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenWriteRepository _refreshTokenWriteRepository;
    private readonly ITenantServiceClient _tenantServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtTokenSettings _jwtTokenSettings;

    public ActivateAccountCommandHandler(
        IActivationTokenReadRepository activationTokenReadRepository,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenWriteRepository refreshTokenWriteRepository,
        ITenantServiceClient tenantServiceClient,
        IUnitOfWork unitOfWork,
        IOptions<JwtTokenSettings> jwtTokenSettings
    )
    {
        _activationTokenReadRepository = activationTokenReadRepository;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenWriteRepository = refreshTokenWriteRepository;
        _tenantServiceClient = tenantServiceClient;
        _unitOfWork = unitOfWork;
        _jwtTokenSettings = jwtTokenSettings.Value;
    }

    public async Task<ResponseLoginDto> Handle(ActivateAccountCommand request, CancellationToken cancellationToken)
    {
        var hashedToken = _tokenProvider.Hash(request.Token);

        var activationToken = await _activationTokenReadRepository.GetActiveByTokenAsync(hashedToken, cancellationToken);

        if (activationToken == null || !activationToken.IsActive || activationToken.User == null)
            throw new ErrorOnValidationException(ResourceMessagesException.TOKEN_INVALID);

        var user = activationToken.User;
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        user.Activate(passwordHash);
        activationToken.Use();

        var workspaces = await _tenantServiceClient.GetUserWorkspacesAsync(user.Id, cancellationToken) ?? [];
        var activeWorkspace = workspaces.FirstOrDefault(w => string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase))
            ?? workspaces.FirstOrDefault();

        Guid activeTenantId = activeWorkspace?.TenantId ?? user.DefaultTenantId ?? Guid.Empty;
        string? activeType = activeWorkspace?.TenantType;
        string? activePlan = activeWorkspace?.Plan;
        string? activeRole = activeWorkspace?.Role;

        if (user.DefaultTenantId is null && activeTenantId != Guid.Empty)
        {
            user.SetDefaultTenantId(activeTenantId);
        }

        var accessToken = _accessTokenGenerator.Generate(
            user,
            activeType,
            activePlan,
            activeTenantId != Guid.Empty ? activeTenantId : null,
            activeRole
        );

        var refreshTokenString = _refreshTokenGenerator.Generate();
        var hashedRefreshToken = _tokenProvider.Hash(refreshTokenString);

        var refreshToken = new RefreshToken(
            hashedRefreshToken,
            DateTime.UtcNow.AddDays(_jwtTokenSettings.RefreshTokenExpirationTimeDays),
            user.Id
        );

        await _refreshTokenWriteRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        ResponseLoginTenantDto? responseTenant = null;
        if (activeWorkspace != null)
        {
            responseTenant = new ResponseLoginTenantDto(
                activeWorkspace.TenantId,
                activeWorkspace.TenantName,
                activeWorkspace.TenantSlug,
                activeWorkspace.TenantType,
                activeWorkspace.Plan,
                activeWorkspace.Role
            );
        }

        return new ResponseLoginDto(
            user.Id,
            user.Name,
            user.Email,
            new ResponseTokensDto(accessToken, refreshTokenString),
            responseTenant,
            workspaces
        );
    }
}
