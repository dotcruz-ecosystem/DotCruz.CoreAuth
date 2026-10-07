using CommonTestUtilities.Data;
using CommonTestUtilities.Security;
using CommonTestUtilities.Services;
using DotCruz.CoreAuth.Application.Commands.Auth.SwitchWorkspace;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;
using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Data;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Moq;

namespace CommonTestUtilities.Commands.Auth;

public class SwitchWorkspaceCommandHandlerBuilder
{
    private ILoggedUser _loggedUser = new LoggedUserBuilder().Build();
    private IWorkspaceServiceClient _workspaceServiceClient = new WorkspaceServiceClientBuilder().Build();
    private IAccessTokenGenerator _accessTokenGenerator = new Mock<IAccessTokenGenerator>().Object;
    private IRefreshTokenGenerator _refreshTokenGenerator = new Mock<IRefreshTokenGenerator>().Object;
    private IRefreshTokenWriteRepository _refreshTokenWriteRepository = new Mock<IRefreshTokenWriteRepository>().Object;
    private ITokenProvider _tokenProvider = new TestTokenProvider();
    private IUnitOfWork _unitOfWork = UnitOfWorkBuilder.Build();
    private IOptions<JwtTokenSettings> _jwtTokenSettings = Options.Create(new JwtTokenSettings { RefreshTokenExpirationTimeDays = 7 });

    public SwitchWorkspaceCommandHandlerBuilder SetLoggedUser(ILoggedUser loggedUser)
    {
        _loggedUser = loggedUser;
        return this;
    }

    public SwitchWorkspaceCommandHandlerBuilder SetWorkspaceServiceClient(IWorkspaceServiceClient workspaceServiceClient)
    {
        _workspaceServiceClient = workspaceServiceClient;
        return this;
    }

    public SwitchWorkspaceCommandHandlerBuilder SetAccessTokenGenerator(IAccessTokenGenerator accessTokenGenerator)
    {
        _accessTokenGenerator = accessTokenGenerator;
        return this;
    }

    public SwitchWorkspaceCommandHandlerBuilder SetRefreshTokenGenerator(IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenGenerator = refreshTokenGenerator;
        return this;
    }

    public SwitchWorkspaceCommandHandler Build()
    {
        return new SwitchWorkspaceCommandHandler(
            _loggedUser,
            _workspaceServiceClient,
            _accessTokenGenerator,
            _refreshTokenGenerator,
            _refreshTokenWriteRepository,
            _tokenProvider,
            _unitOfWork,
            _jwtTokenSettings
        );
    }
}
