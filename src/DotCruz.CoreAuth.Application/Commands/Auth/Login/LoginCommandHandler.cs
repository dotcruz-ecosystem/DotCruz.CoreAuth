using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotCruz.CoreAuth.Application.Commands.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ResponseLoginDto>
    {
        private readonly IUserReadRepository _userReadRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccessTokenGenerator _accessTokenGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IRefreshTokenWriteRepository _refreshTokenWriteRepository;
        private readonly ITokenProvider _tokenProvider;
        private readonly ITenantServiceClient _tenantServiceClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenSettings _jwtTokenSettings;

        public LoginCommandHandler(
            IUserReadRepository userReadRepository,
            IPasswordHasher passwordHasher,
            IAccessTokenGenerator accessTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IRefreshTokenWriteRepository refreshTokenWriteRepository,
            ITokenProvider tokenProvider,
            ITenantServiceClient tenantServiceClient,
            IUnitOfWork unitOfWork,
            IOptions<JwtTokenSettings> jwtTokenSettings)
        {
            _userReadRepository = userReadRepository;
            _passwordHasher = passwordHasher;
            _accessTokenGenerator = accessTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokenWriteRepository = refreshTokenWriteRepository;
            _tokenProvider = tokenProvider;
            _tenantServiceClient = tenantServiceClient;
            _unitOfWork = unitOfWork;
            _jwtTokenSettings = jwtTokenSettings.Value;
        }

        public async Task<ResponseLoginDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var email = request.Email.ToLowerInvariant();

            var user = await _userReadRepository.GetUserByEmailAsync(email, cancellationToken) 
                ?? throw new InvalidLoginException();

            var passwordMatch = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash!);

            if (!passwordMatch)
                throw new InvalidLoginException();

            var tenantSummary = await _tenantServiceClient.GetTenantSummaryAsync(user.TenantId, cancellationToken);

            var accessToken = _accessTokenGenerator.Generate(user, tenantSummary?.Type, tenantSummary?.Plan);
            var refreshTokenString = _refreshTokenGenerator.Generate();
            var hashedRefreshToken = _tokenProvider.Hash(refreshTokenString);

            var refreshToken = new RefreshToken(
                hashedRefreshToken,
                DateTime.UtcNow.AddDays(_jwtTokenSettings.RefreshTokenExpirationTimeDays),
                user.Id
            );

            await _refreshTokenWriteRepository.AddAsync(refreshToken, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return new ResponseLoginDto(
                user.Id,
                user.Name,
                user.Email,
                new ResponseTokensDto(accessToken, refreshTokenString)
            );
        }
    }
}
