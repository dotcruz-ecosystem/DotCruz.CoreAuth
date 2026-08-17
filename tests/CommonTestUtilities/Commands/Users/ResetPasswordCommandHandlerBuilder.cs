using CommonTestUtilities.Data;
using CommonTestUtilities.Repositories.Tokens;
using CommonTestUtilities.Security;
using DotCruz.CoreAuth.Application.Commands.Auth.PasswordResetTokens.ResetPassword;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Moq;

namespace CommonTestUtilities.Commands.Users
{
    public class ResetPasswordCommandHandlerBuilder
    {
        private IPasswordResetTokenReadRepository _tokenReadRepository = new Mock<IPasswordResetTokenReadRepository>().Object;
        private IRefreshTokenReadRepository _refreshTokenReadRepository = new RefreshTokenReadRepositoryBuilder().Build();
        private IPasswordHasher _passwordHasher = new PasswordHasherBuilder().Build();
        private ITokenProvider _tokenProvider = new Mock<ITokenProvider>().Object;
        private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();

        public ResetPasswordCommandHandlerBuilder SetTokenReadRepository(IPasswordResetTokenReadRepository tokenReadRepository)
        {
            _tokenReadRepository = tokenReadRepository;
            return this;
        }

        public ResetPasswordCommandHandlerBuilder SetRefreshTokenReadRepository(IRefreshTokenReadRepository refreshTokenReadRepository)
        {
            _refreshTokenReadRepository = refreshTokenReadRepository;
            return this;
        }

        public ResetPasswordCommandHandlerBuilder SetTokenProvider(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
            return this;
        }

        public ResetPasswordCommandHandler Build()
        {
            return new ResetPasswordCommandHandler(
                _tokenReadRepository,
                _refreshTokenReadRepository,
                _passwordHasher,
                _tokenProvider,
                _unitOfWork
            );
        }
    }
}
