using CommonTestUtilities.Data;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Security;
using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Moq;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;

namespace CommonTestUtilities.Commands.Users
{
    public class LoginCommandHandlerBuilder
    {
        private IUserReadRepository _userReadRepository = new UserReadRepositoryBuilder().Build();
        private IPasswordHasher _passwordHasher = new PasswordHasherBuilder().Build();
        private IAccessTokenGenerator _accessTokenGenerator = new Mock<IAccessTokenGenerator>().Object;
        private IRefreshTokenGenerator _refreshTokenGenerator = new Mock<IRefreshTokenGenerator>().Object;
        private IRefreshTokenWriteRepository _refreshTokenWriteRepository = new Mock<IRefreshTokenWriteRepository>().Object;
        private ITokenProvider _tokenProvider = new TestTokenProvider();
        private ITenantServiceClient _tenantServiceClient = new Mock<ITenantServiceClient>().Object;
        private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();
        private IOptions<JwtTokenSettings> _jwtTokenSettings = Options.Create(new JwtTokenSettings { RefreshTokenExpirationTimeDays = 7 });

        public LoginCommandHandlerBuilder SetUserReadRepository(IUserReadRepository userReadRepository)
        {
            _userReadRepository = userReadRepository;
            return this;
        }

        public LoginCommandHandlerBuilder SetPasswordHasher(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
            return this;
        }

        public LoginCommandHandlerBuilder SetAccessTokenGenerator(IAccessTokenGenerator accessTokenGenerator)
        {
            _accessTokenGenerator = accessTokenGenerator;
            return this;
        }

        public LoginCommandHandlerBuilder SetRefreshTokenGenerator(IRefreshTokenGenerator refreshTokenGenerator)
        {
            _refreshTokenGenerator = refreshTokenGenerator;
            return this;
        }

        public LoginCommandHandlerBuilder SetTokenProvider(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
            return this;
        }

        public LoginCommandHandlerBuilder SetTenantServiceClient(ITenantServiceClient tenantServiceClient)
        {
            _tenantServiceClient = tenantServiceClient;
            return this;
        }

        public LoginCommandHandler Build()
        {
            return new LoginCommandHandler(
                _userReadRepository,
                _passwordHasher,
                _accessTokenGenerator,
                _refreshTokenGenerator,
                _refreshTokenWriteRepository,
                _tokenProvider,
                _tenantServiceClient,
                _unitOfWork,
                _jwtTokenSettings
            );
        }
    }
}
