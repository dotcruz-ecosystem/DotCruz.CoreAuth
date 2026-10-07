using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;
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
        private readonly IWorkspaceServiceClient _workspaceServiceClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenSettings _jwtTokenSettings;

        public LoginCommandHandler(
            IUserReadRepository userReadRepository,
            IPasswordHasher passwordHasher,
            IAccessTokenGenerator accessTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IRefreshTokenWriteRepository refreshTokenWriteRepository,
            ITokenProvider tokenProvider,
            IWorkspaceServiceClient workspaceServiceClient,
            IUnitOfWork unitOfWork,
            IOptions<JwtTokenSettings> jwtTokenSettings)
        {
            _userReadRepository = userReadRepository;
            _passwordHasher = passwordHasher;
            _accessTokenGenerator = accessTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokenWriteRepository = refreshTokenWriteRepository;
            _tokenProvider = tokenProvider;
            _workspaceServiceClient = workspaceServiceClient;
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

            var workspaces = await _workspaceServiceClient.GetUserWorkspacesAsync(user.Id, cancellationToken) ?? [];

            UserWorkspaceDto? activeWorkspace = null;

            if (request.PreferredWorkspaceId.HasValue)
            {
                activeWorkspace = workspaces.FirstOrDefault(w => 
                    w.WorkspaceId == request.PreferredWorkspaceId.Value && 
                    string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase));
            }

            if (activeWorkspace is null && workspaces.Count > 0)
            {
                activeWorkspace = workspaces.FirstOrDefault(w => 
                    w.WorkspaceId == user.WorkspaceId && 
                    string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase))
                    ?? workspaces.FirstOrDefault(w => 
                    string.Equals(w.MemberStatus, "Active", StringComparison.OrdinalIgnoreCase))
                    ?? workspaces.First();
            }

            Guid activeWorkspaceId = activeWorkspace?.WorkspaceId ?? user.WorkspaceId;
            string? activeType = activeWorkspace?.WorkspaceType;
            string? activePlan = activeWorkspace?.Plan;
            string? activeRole = activeWorkspace?.Role;

            WorkspaceSummaryDto? workspaceSummary = null;
            if (activeWorkspace is null && activeWorkspaceId != Guid.Empty)
            {
                workspaceSummary = await _workspaceServiceClient.GetWorkspaceSummaryAsync(activeWorkspaceId, cancellationToken);
                activeType = workspaceSummary?.Type;
                activePlan = workspaceSummary?.Plan;
            }

            var accessToken = _accessTokenGenerator.Generate(
                user, 
                activeType, 
                activePlan, 
                activeWorkspaceId != Guid.Empty && activeWorkspaceId != user.WorkspaceId ? activeWorkspaceId : null, 
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
            else if (workspaceSummary != null)
            {
                responseWorkspace = new ResponseLoginWorkspaceDto(
                    workspaceSummary.Id,
                    workspaceSummary.Name,
                    workspaceSummary.Slug,
                    workspaceSummary.Type,
                    workspaceSummary.Plan
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
}
