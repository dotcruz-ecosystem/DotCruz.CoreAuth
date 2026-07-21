using CommonTestUtilities.Data;
using CommonTestUtilities.Repositories.Tokens;
using CommonTestUtilities.Security;
using DotCruz.CoreAuth.Application.Commands.Auth.RefreshTokens;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Moq;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;

namespace CommonTestUtilities.Commands.Auth;

public class RefreshTokenCommandHandlerBuilder
{
    private IRefreshTokenReadRepository _refreshTokenReadRepository = new RefreshTokenReadRepositoryBuilder().Build();
    private IRefreshTokenWriteRepository _refreshTokenWriteRepository = new RefreshTokenWriteRepositoryBuilder().Build();
    private IAccessTokenGenerator _accessTokenGenerator = new Mock<IAccessTokenGenerator>().Object;
    private IRefreshTokenGenerator _refreshTokenGenerator = new Mock<IRefreshTokenGenerator>().Object;
    private ITokenProvider _tokenProvider = new TestTokenProvider();
    private ITenantServiceClient _tenantServiceClient = new Mock<ITenantServiceClient>().Object;
    private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();
    private IOptions<JwtTokenSettings> _jwtTokenSettings = Options.Create(new JwtTokenSettings { RefreshTokenExpirationTimeDays = 7 });

    public RefreshTokenCommandHandlerBuilder SetRefreshTokenReadRepository(IRefreshTokenReadRepository readRepository)
    {
        _refreshTokenReadRepository = readRepository;
        return this;
    }

    public RefreshTokenCommandHandlerBuilder SetRefreshTokenWriteRepository(IRefreshTokenWriteRepository writeRepository)
    {
        _refreshTokenWriteRepository = writeRepository;
        return this;
    }

    public RefreshTokenCommandHandlerBuilder SetAccessTokenGenerator(IAccessTokenGenerator accessTokenGenerator)
    {
        _accessTokenGenerator = accessTokenGenerator;
        return this;
    }

    public RefreshTokenCommandHandlerBuilder SetRefreshTokenGenerator(IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenGenerator = refreshTokenGenerator;
        return this;
    }

    public RefreshTokenCommandHandlerBuilder SetTokenProvider(ITokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
        return this;
    }

    public RefreshTokenCommandHandlerBuilder SetTenantServiceClient(ITenantServiceClient tenantServiceClient)
    {
        _tenantServiceClient = tenantServiceClient;
        return this;
    }

    public RefreshTokenCommandHandlerBuilder SetUnitOfWork(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        return this;
    }

    public RefreshTokenCommandHandlerBuilder SetJwtTokenSettings(IOptions<JwtTokenSettings> jwtTokenSettings)
    {
        _jwtTokenSettings = jwtTokenSettings;
        return this;
    }

    public RefreshTokenCommandHandler Build()
    {
        return new RefreshTokenCommandHandler(
            _refreshTokenReadRepository,
            _refreshTokenWriteRepository,
            _accessTokenGenerator,
            _refreshTokenGenerator,
            _tokenProvider,
            _tenantServiceClient,
            _unitOfWork,
            _jwtTokenSettings
        );
    }
}
