using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;
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
    private readonly IWorkspaceServiceClient _workspaceServiceClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtTokenSettings _jwtTokenSettings;

    public ActivateAccountCommandHandler(
        IActivationTokenReadRepository activationTokenReadRepository,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IRefreshTokenWriteRepository refreshTokenWriteRepository,
        IWorkspaceServiceClient workspaceServiceClient,
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
        _workspaceServiceClient = workspaceServiceClient;
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

        var workspaces = await _workspaceServiceClient.GetUserWorkspacesAsync(user.Id, cancellationToken) ?? [];
        var activeWorkspace = workspaces.FirstOrDefault(w => string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase))
            ?? workspaces.FirstOrDefault();

        Guid activeWorkspaceId = activeWorkspace?.WorkspaceId ?? user.DefaultWorkspaceId ?? Guid.Empty;
        string? activeType = activeWorkspace?.WorkspaceType;
        string? activePlan = activeWorkspace?.Plan;
        string? activeRole = activeWorkspace?.Role;

        if (user.DefaultWorkspaceId is null && activeWorkspaceId != Guid.Empty)
        {
            user.SetDefaultWorkspaceId(activeWorkspaceId);
        }

        var accessToken = _accessTokenGenerator.Generate(
            user,
            activeType,
            activePlan,
            activeWorkspaceId != Guid.Empty ? activeWorkspaceId : null,
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

        ResponseLoginWorkspaceDto? responseWorkspace = null;
        if (activeWorkspace != null)
        {
            responseWorkspace = new ResponseLoginWorkspaceDto(
                activeWorkspace.WorkspaceId,
                activeWorkspace.WorkspaceName,
                activeWorkspace.WorkspaceSlug,
                activeWorkspace.WorkspaceType,
                activeWorkspace.Plan,
                activeWorkspace.Role
            );
        }

        return new ResponseLoginDto(
            user.Id,
            user.Name,
            user.Email,
            new ResponseTokensDto(accessToken, refreshTokenString),
            responseWorkspace,
            workspaces
        );
    }
}
