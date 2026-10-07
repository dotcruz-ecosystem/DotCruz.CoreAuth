using CommonTestUtilities.Commands.Auth.ActivateAccount;
using CommonTestUtilities.Entities.Tokens;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Repositories.Tokens;
using CommonTestUtilities.Requests.Auth.ActivateAccount;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Command.Test.Auth.ActivateAccount;

public class ActivateAccountCommandHandlerTest
{
    [Fact]
    public async Task Success()
    {
        var command = ActivateAccountCommandBuilder.Build();
        var user = UserBuilder.Build(passwordHashed: null, status: UserStatus.PendingActivation);
        var token = ActivationTokenBuilder.Build(token: "hashed-token", userId: user.Id);

        typeof(ActivationToken)
            .GetProperty("User")?.SetValue(token, user);

        var tokenReadRepository = new ActivationTokenReadRepositoryBuilder()
            .SetupGetActiveByToken("hashed-token", token)
            .Build();

        var tokenProvider = new Mock<ITokenProvider>();
        tokenProvider.Setup(t => t.Hash(It.IsAny<string>())).Returns("hashed-token");

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(p => p.HashPassword(command.Password)).Returns("hashed-new-password");

        var handler = new ActivateAccountCommandHandlerBuilder()
            .SetActivationTokenReadRepository(tokenReadRepository)
            .SetTokenProvider(tokenProvider.Object)
            .SetPasswordHasher(passwordHasher.Object)
            .Build();

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        user.Status.Should().Be(UserStatus.Active);
        user.PasswordHash.Should().Be("hashed-new-password");
        token.IsUsed.Should().BeTrue();
        token.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Error_Token_NotFound_Or_Inactive()
    {
        var command = ActivateAccountCommandBuilder.Build();

        var tokenReadRepository = new ActivationTokenReadRepositoryBuilder()
            .SetupGetActiveByToken(It.IsAny<string>(), null)
            .Build();

        var tokenProvider = new Mock<ITokenProvider>();
        tokenProvider.Setup(t => t.Hash(It.IsAny<string>())).Returns("hashed-token");

        var handler = new ActivateAccountCommandHandlerBuilder()
            .SetActivationTokenReadRepository(tokenReadRepository)
            .SetTokenProvider(tokenProvider.Object)
            .Build();

        Func<Task> act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<ErrorOnValidationException>();
        exception.And.GetErrorsMessages().Should().Contain(ResourceMessagesException.TOKEN_INVALID);
    }
}
