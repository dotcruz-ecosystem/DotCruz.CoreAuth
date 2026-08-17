using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using MediatR;

namespace DotCruz.CoreAuth.Application.Commands.Auth.PasswordResetTokens.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IPasswordResetTokenReadRepository _tokenReadRepository;
        private readonly IRefreshTokenReadRepository _refreshTokenReadRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenProvider _tokenProvider;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordCommandHandler(
            IPasswordResetTokenReadRepository tokenReadRepository,
            IRefreshTokenReadRepository refreshTokenReadRepository,
            IPasswordHasher passwordHasher,
            ITokenProvider tokenProvider,
            IUnitOfWork unitOfWork)
        {
            _tokenReadRepository = tokenReadRepository;
            _refreshTokenReadRepository = refreshTokenReadRepository;
            _passwordHasher = passwordHasher;
            _tokenProvider = tokenProvider;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var hashedToken = _tokenProvider.Hash(request.Token);
            var token = await _tokenReadRepository.GetByTokenAsync(hashedToken, cancellationToken);

            if (token is null || token.IsExpired || token.User is null)
                throw new NotFoundException(ResourceMessagesException.TOKEN_INVALID);

            var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

            token.User.Update(null, null, newPasswordHash, null);
            token.MarkAsUsed();

            var activeRefreshTokens = await _refreshTokenReadRepository
                .GetActiveTokensByUserIdAsync(token.UserId, cancellationToken);

            foreach (var refreshToken in activeRefreshTokens ?? [])
                refreshToken.Revoke();

            await _unitOfWork.CommitAsync(cancellationToken);
        }
    }
}
