using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotCruz.CoreAuth.Application.Commands.Auth.RefreshTokens;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ResponseTokensDto>
{
    private readonly IRefreshTokenReadRepository _refreshTokenReadRepository;
    private readonly IRefreshTokenWriteRepository _refreshTokenWriteRepository;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly ITokenProvider _tokenProvider;
    private readonly ITenantServiceClient _tenantServiceClient;
    private IUnitOfWork _unitOfWork;
    private readonly JwtTokenSettings _jwtTokenSettings;

    public RefreshTokenCommandHandler(
        IRefreshTokenReadRepository refreshTokenReadRepository,
        IRefreshTokenWriteRepository refreshTokenWriteRepository,
        IAccessTokenGenerator accessTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        ITokenProvider tokenProvider,
        ITenantServiceClient tenantServiceClient,
        IUnitOfWork unitOfWork,
        IOptions<JwtTokenSettings> jwtTokenSettings
    )
    {
        _refreshTokenReadRepository = refreshTokenReadRepository;
        _refreshTokenWriteRepository = refreshTokenWriteRepository;
        _accessTokenGenerator = accessTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _tokenProvider = tokenProvider;
        _tenantServiceClient = tenantServiceClient;
        _unitOfWork = unitOfWork;
        _jwtTokenSettings = jwtTokenSettings.Value;
    }

    public async Task<ResponseTokensDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashedToken = _tokenProvider.Hash(request.RefreshToken);
        var refreshToken = await _refreshTokenReadRepository.GetByTokenAsync(hashedToken, cancellationToken);

        if (refreshToken is not null && refreshToken.IsRevoked)
        {
            await RevokeAllActiveTokens(refreshToken.UserId, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            throw new ErrorOnValidationException(ResourceMessagesException.TOKEN_INVALID);
        }

        ValidateToken(refreshToken);

        refreshToken!.Revoke();

        var tenantSummary = await _tenantServiceClient.GetTenantSummaryAsync(refreshToken.User!.TenantId, cancellationToken);

        var newAccessToken = _accessTokenGenerator.Generate(refreshToken.User!, tenantSummary?.Type, tenantSummary?.Plan);

        var newRefreshTokenString = await CreateNewRefreshToken(refreshToken.User!.Id, cancellationToken);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return new ResponseTokensDto(newAccessToken, newRefreshTokenString);
    }

    private static void ValidateToken(RefreshToken? refreshToken)
    {
        var errors = new List<string>();

        if (refreshToken == null || !refreshToken.IsActive)
            errors.Add(ResourceMessagesException.TOKEN_INVALID);

        if (refreshToken != null && refreshToken.User?.CanAuthenticate != true)
            errors.Add(ResourceMessagesException.USER_NOT_FOUND);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }

    private async Task RevokeAllActiveTokens(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await _refreshTokenReadRepository.GetActiveTokensByUserIdAsync(userId, cancellationToken);

        foreach (var activeToken in activeTokens ?? [])
            activeToken.Revoke();
    }

    private async Task<string> CreateNewRefreshToken(Guid userId, CancellationToken cancellationToken)
    {
        var newRefreshTokenString = _refreshTokenGenerator.Generate();
        var hashedRefreshToken = _tokenProvider.Hash(newRefreshTokenString);

        var newRefreshToken = new RefreshToken(
            hashedRefreshToken,
            DateTimeOffset.UtcNow.AddDays(_jwtTokenSettings.RefreshTokenExpirationTimeDays),
            userId
        );

        await _refreshTokenWriteRepository.AddAsync(newRefreshToken, cancellationToken);

        return newRefreshTokenString;
    }
}
