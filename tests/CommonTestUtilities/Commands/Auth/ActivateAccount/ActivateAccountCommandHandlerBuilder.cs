using CommonTestUtilities.Data;
using CommonTestUtilities.Repositories.Tokens;
using CommonTestUtilities.Security;
using CommonTestUtilities.Services;
using DotCruz.CoreAuth.Application.Commands.Auth.ActivateAccount;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Moq;

namespace CommonTestUtilities.Commands.Auth.ActivateAccount;

public class ActivateAccountCommandHandlerBuilder
{
    private IActivationTokenReadRepository _activationTokenReadRepository = new ActivationTokenReadRepositoryBuilder().Build();
    private IPasswordHasher _passwordHasher = new PasswordHasherBuilder().Build();
    private ITokenProvider _tokenProvider = new TestTokenProvider();
    private IAccessTokenGenerator _accessTokenGenerator = DefaultAccessTokenGenerator();
    private IRefreshTokenGenerator _refreshTokenGenerator = DefaultRefreshTokenGenerator();
    private IRefreshTokenWriteRepository _refreshTokenWriteRepository = new Mock<IRefreshTokenWriteRepository>().Object;
    private IWorkspaceServiceClient _workspaceServiceClient = new WorkspaceServiceClientBuilder().Build();
    private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();
    private IOptions<JwtTokenSettings> _jwtTokenSettings = Options.Create(new JwtTokenSettings { RefreshTokenExpirationTimeDays = 7 });

    private static IRefreshTokenGenerator DefaultRefreshTokenGenerator()
    {
        var mock = new Mock<IRefreshTokenGenerator>();
        mock.Setup(m => m.Generate()).Returns("valid-refresh-token");
        return mock.Object;
    }

    private static IAccessTokenGenerator DefaultAccessTokenGenerator()
    {
        var mock = new Mock<IAccessTokenGenerator>();
        mock.Setup(m => m.Generate(It.IsAny<DotCruz.CoreAuth.Domain.Entities.Users.User>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>()))
            .Returns("valid-access-token");
        return mock.Object;
    }

    public ActivateAccountCommandHandlerBuilder SetActivationTokenReadRepository(IActivationTokenReadRepository readRepository)
    {
        _activationTokenReadRepository = readRepository;
        return this;
    }

    public ActivateAccountCommandHandlerBuilder SetPasswordHasher(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
        return this;
    }

    public ActivateAccountCommandHandlerBuilder SetTokenProvider(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
        return this;
    }

    public ActivateAccountCommandHandlerBuilder SetAccessTokenGenerator(IAccessTokenGenerator accessTokenGenerator)
    {
        _accessTokenGenerator = accessTokenGenerator;
        return this;
    }

    public ActivateAccountCommandHandlerBuilder SetRefreshTokenGenerator(IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenGenerator = refreshTokenGenerator;
        return this;
    }

    public ActivateAccountCommandHandlerBuilder SetRefreshTokenWriteRepository(IRefreshTokenWriteRepository writeRepository)
    {
        _refreshTokenWriteRepository = writeRepository;
        return this;
    }

    public ActivateAccountCommandHandlerBuilder SetWorkspaceServiceClient(IWorkspaceServiceClient workspaceServiceClient)
    {
        _workspaceServiceClient = workspaceServiceClient;
        return this;
    }

    public ActivateAccountCommandHandlerBuilder SetUnitOfWork(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        return this;
    }

    public ActivateAccountCommandHandler Build()
    {
        return new ActivateAccountCommandHandler(
            _activationTokenReadRepository,
            _passwordHasher,
            _tokenProvider,
            _accessTokenGenerator,
            _refreshTokenGenerator,
            _refreshTokenWriteRepository,
            _workspaceServiceClient,
            _unitOfWork,
            _jwtTokenSettings
        );
    }
}
