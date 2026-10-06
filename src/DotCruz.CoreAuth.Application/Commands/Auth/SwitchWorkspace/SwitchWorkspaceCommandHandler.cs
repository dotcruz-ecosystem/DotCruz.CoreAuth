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

namespace DotCruz.CoreAuth.Application.Commands.Auth.SwitchWorkspace;

public class SwitchWorkspaceCommandHandler : IRequestHandler<SwitchWorkspaceCommand, ResponseLoginDto>
{
    private readonly ILoggedUser _loggedUser;
    private readonly ITenantServiceClient _tenantServiceClient;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IRefreshTokenWriteRepository _refreshTokenWriteRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtTokenSettings _jwtTokenSettings;

    public SwitchWorkspaceCommandHandler(
        ILoggedUser loggedUser,
        ITenantServiceClient tenantServiceClient,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenWriteRepository refreshTokenWriteRepository,
        ITokenProvider tokenProvider,
        IUnitOfWork unitOfWork,
        IOptions<JwtTokenSettings> jwtTokenSettings)
    {
        _loggedUser = loggedUser;
        _tenantServiceClient = tenantServiceClient;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _refreshTokenWriteRepository = refreshTokenWriteRepository;
        _tokenProvider = tokenProvider;
        _unitOfWork = unitOfWork;
        _jwtTokenSettings = jwtTokenSettings.Value;
    }

    public async Task<ResponseLoginDto> Handle(SwitchWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var user = await _loggedUser.User(cancellationToken);

        var workspaces = await _tenantServiceClient.GetUserWorkspacesAsync(user.Id, cancellationToken);

        var targetWorkspace = workspaces.FirstOrDefault(w => w.TenantId == request.TargetTenantId)
            ?? throw new NotFoundException(ResourceMessagesException.WORKSPACE_NOT_FOUND);

        if (!string.Equals(targetWorkspace.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase))
            throw new ErrorOnValidationException(ResourceMessagesException.WORKSPACE_NOT_MEMBER_OR_INACTIVE);

        var accessToken = _accessTokenGenerator.Generate(
            user,
            targetWorkspace.TenantType,
            targetWorkspace.Plan,
            targetWorkspace.TenantId,
            targetWorkspace.Role);

        var refreshTokenString = _refreshTokenGenerator.Generate();
        var hashedRefreshToken = _tokenProvider.Hash(refreshTokenString);

        var refreshToken = new RefreshToken(
            hashedRefreshToken,
            DateTime.UtcNow.AddDays(_jwtTokenSettings.RefreshTokenExpirationTimeDays),
            user.Id
        );

        await _refreshTokenWriteRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var responseTenant = new ResponseLoginTenantDto(
            targetWorkspace.TenantId,
            targetWorkspace.TenantName,
            targetWorkspace.TenantSlug,
            targetWorkspace.TenantType,
            targetWorkspace.Plan,
            targetWorkspace.Role
        );

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
