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

            if (!user.CanAuthenticate)
                throw new InvalidLoginException();

            var passwordMatch = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash!);

            if (!passwordMatch)
                throw new InvalidLoginException();

            var workspaces = await _tenantServiceClient.GetUserWorkspacesAsync(user.Id, cancellationToken) ?? [];

            DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses.UserWorkspaceDto? activeWorkspace = null;

            if (request.PreferredTenantId.HasValue)
            {
                activeWorkspace = workspaces.FirstOrDefault(w => 
                    w.TenantId == request.PreferredTenantId.Value && 
                    string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase));
            }

            if (activeWorkspace is null && workspaces.Count > 0)
            {
                activeWorkspace = workspaces.FirstOrDefault(w => 
                    w.TenantId == user.TenantId && 
                    string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase))
                    ?? workspaces.FirstOrDefault(w => 
                    string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase))
                    ?? workspaces.First();
            }

            Guid activeTenantId = activeWorkspace?.TenantId ?? user.TenantId;
            string? activeType = activeWorkspace?.TenantType;
            string? activePlan = activeWorkspace?.Plan;
            string? activeRole = activeWorkspace?.Role;

            DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses.TenantSummaryDto? tenantSummary = null;
            if (activeWorkspace is null && activeTenantId != Guid.Empty)
            {
                tenantSummary = await _tenantServiceClient.GetTenantSummaryAsync(activeTenantId, cancellationToken);
                activeType = tenantSummary?.Type;
                activePlan = tenantSummary?.Plan;
            }

            var accessToken = _accessTokenGenerator.Generate(
                user, 
                activeType, 
                activePlan, 
                activeTenantId != Guid.Empty && activeTenantId != user.TenantId ? activeTenantId : null, 
                activeRole);

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
            else if (tenantSummary != null)
            {
                responseTenant = new ResponseLoginTenantDto(
                    tenantSummary.Id,
                    tenantSummary.Name,
                    tenantSummary.Slug,
                    tenantSummary.Type,
                    tenantSummary.Plan
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
}
