using CommonTestUtilities.Commands.Users;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Command.Test.Users.CreateUser;

public class CreateUserHandlerTest
{
    [Fact]
    public async Task Success()
    {
        var command = CreateUserCommandBuilder.Build();

        var userWriteRepositoryMock = new Mock<IUserWriteRepository>();
        User? savedUser = null;
        userWriteRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => savedUser = u);

        var tokenProviderMock = new Mock<ITokenProvider>();
        tokenProviderMock.Setup(x => x.Value()).Returns("plain-token");
        tokenProviderMock.Setup(x => x.Hash("plain-token")).Returns("hashed-token");

        var activationTokenWriteRepositoryMock = new Mock<IActivationTokenWriteRepository>();
        ActivationToken? savedToken = null;
        activationTokenWriteRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ActivationToken>(), It.IsAny<CancellationToken>()))
            .Callback<ActivationToken, CancellationToken>((t, _) => savedToken = t);

        var emailServiceMock = new Mock<IEmailService>();

        var handler = new CreateUserCommandHandlerBuilder()
            .SetUserWriteRepository(userWriteRepositoryMock.Object)
            .SetTokenProvider(tokenProviderMock.Object)
            .SetActivationTokenWriteRepository(activationTokenWriteRepositoryMock.Object)
            .SetEmailService(emailServiceMock.Object)
            .Build();

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.Should().NotBeEmpty();
        savedUser.Should().NotBeNull();
        savedUser!.Status.Should().Be(UserStatus.PendingActivation);
        savedUser.PasswordHash.Should().BeNull();
        
        savedToken.Should().NotBeNull();
        savedToken!.Token.Should().Be("hashed-token");
        savedToken.UserId.Should().Be(savedUser.Id);

        emailServiceMock.Verify(x => x.SendActivationEmailAsync(
            command.Email.ToLowerInvariant(), 
            command.Name, 
            "plain-token", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Error_Email_Already_Registered()
    {
        var command = CreateUserCommandBuilder.Build();

        var userReadRepository = new UserReadRepositoryBuilder()
            .SetupExistsActiveUserWithEmail(command.Email, true)
            .Build();

        var handler = new CreateUserCommandHandlerBuilder()
            .SetUserReadRepository(userReadRepository)
            .Build();

        Func<Task> act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<ErrorOnValidationException>();
        
        exception.And.GetErrorsMessages().Should().Contain(ResourceMessagesException.EMAIL_ALREADY_EXISTS)
            .And.HaveCount(1);
    }
}
