using CommonTestUtilities.Commands.Auth;
using CommonTestUtilities.Entities.Tokens;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Repositories.Tokens;
using CommonTestUtilities.Requests.Auth;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using FluentAssertions;
using Moq;
using RefreshTokenEntity = DotCruz.CoreAuth.Domain.Entities.Tokens.RefreshToken;

namespace Command.Test.Auth.RevokeAllUserTokens;

public class RevokeAllUserTokensCommandHandlerTests
{
    [Fact]
    public async Task Success()
    {
        var user = UserBuilder.Build();
        var command = RevokeAllUserTokensCommandBuilder.Build();
        var token1 = RefreshTokenBuilder.Build(userId: user.Id);
        var token2 = RefreshTokenBuilder.Build(userId: user.Id);
        var activeTokens = new List<RefreshTokenEntity> { token1, token2 };

        var readRepository = new RefreshTokenReadRepositoryBuilder()
            .SetupGetActiveTokensByUserId(user.Id, activeTokens)
            .Build();

        var loggedUserMock = new Mock<ILoggedUser>();
        loggedUserMock.Setup(l => l.User(It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new RevokeAllUserTokensCommandHandlerBuilder()
            .SetRefreshTokenReadRepository(readRepository)
            .SetLoggedUser(loggedUserMock.Object)
            .Build();

        await handler.Handle(command, TestContext.Current.CancellationToken);

        token1.IsActive.Should().BeFalse();
        token1.RevokedAt.Should().NotBeNull();
        
        token2.IsActive.Should().BeFalse();
        token2.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Success_NoActiveTokens()
    {
        var user = UserBuilder.Build();
        var command = RevokeAllUserTokensCommandBuilder.Build();
        var activeTokens = new List<RefreshTokenEntity>(); // Vazia

        var readRepository = new RefreshTokenReadRepositoryBuilder()
            .SetupGetActiveTokensByUserId(user.Id, activeTokens)
            .Build();

        var loggedUserMock = new Mock<ILoggedUser>();
        loggedUserMock.Setup(l => l.User(It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var handler = new RevokeAllUserTokensCommandHandlerBuilder()
            .SetRefreshTokenReadRepository(readRepository)
            .SetLoggedUser(loggedUserMock.Object)
            .Build();

        var act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }
}
