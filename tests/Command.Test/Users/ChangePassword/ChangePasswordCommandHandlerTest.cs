using CommonTestUtilities.Entities.Tokens;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Repositories.Users;
using DotCruz.CoreAuth.Application.Commands.Users.ChangePassword;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Security;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CommonTestUtilities.Repositories.Tokens;

namespace Command.Test.Users.ChangePassword;

public class ChangePasswordCommandHandlerTest
{
    [Fact]
    public async Task Success()
    {
        var user = UserBuilder.Build(passwordHashed: "old-hash");
        var userWriteRepository = new UserWriteRepositoryBuilder()
            .SetupGetByIdToUpdate(user)
            .Build();

        var loggedUserMock = new Mock<ILoggedUser>();
        loggedUserMock
            .Setup(x => x.User(It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(x => x.VerifyPassword("current-pass", "old-hash"))
            .Returns(true);
        passwordHasherMock
            .Setup(x => x.HashPassword("new-pass"))
            .Returns("new-hash");

        var unitOfWorkMock = new Mock<DotCruz.CoreAuth.Domain.Interfaces.Data.IUnitOfWork>();

        var handler = new ChangePasswordCommandHandler(
            userWriteRepository,
            new RefreshTokenReadRepositoryBuilder().Build(),
            loggedUserMock.Object,
            passwordHasherMock.Object,
            unitOfWorkMock.Object
        );

        var command = new ChangePasswordCommand("current-pass", "new-pass");

        await handler.Handle(command, CancellationToken.None);

        user.PasswordHash.Should().Be("new-hash");
        unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Error_User_NotFound()
    {
        var user = UserBuilder.Build();
        var userWriteRepository = new UserWriteRepositoryBuilder()
            .SetupGetByIdToUpdate(user.Id, null)
            .Build();

        var loggedUserMock = new Mock<ILoggedUser>();
        loggedUserMock
            .Setup(x => x.User(It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        var unitOfWorkMock = new Mock<DotCruz.CoreAuth.Domain.Interfaces.Data.IUnitOfWork>();

        var handler = new ChangePasswordCommandHandler(
            userWriteRepository,
            new RefreshTokenReadRepositoryBuilder().Build(),
            loggedUserMock.Object,
            passwordHasherMock.Object,
            unitOfWorkMock.Object
        );

        var command = new ChangePasswordCommand("current-pass", "new-pass");

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.And.Message.Should().Be(ResourceMessagesException.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Error_Invalid_Password()
    {
        var user = UserBuilder.Build(passwordHashed: "old-hash");
        var userWriteRepository = new UserWriteRepositoryBuilder()
            .SetupGetByIdToUpdate(user)
            .Build();

        var loggedUserMock = new Mock<ILoggedUser>();
        loggedUserMock
            .Setup(x => x.User(It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(x => x.VerifyPassword("wrong-pass", "old-hash"))
            .Returns(false);

        var unitOfWorkMock = new Mock<DotCruz.CoreAuth.Domain.Interfaces.Data.IUnitOfWork>();

        var handler = new ChangePasswordCommandHandler(
            userWriteRepository,
            new RefreshTokenReadRepositoryBuilder().Build(),
            loggedUserMock.Object,
            passwordHasherMock.Object,
            unitOfWorkMock.Object
        );

        var command = new ChangePasswordCommand("wrong-pass", "new-pass");

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidLoginException>();
    }

    [Fact]
    public async Task Success_Revokes_Active_Refresh_Tokens()
    {
        var user = UserBuilder.Build(passwordHashed: "old-hash");
        var userWriteRepository = new UserWriteRepositoryBuilder()
            .SetupGetByIdToUpdate(user)
            .Build();

        var loggedUserMock = new Mock<ILoggedUser>();
        loggedUserMock
            .Setup(x => x.User(It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(x => x.VerifyPassword("current-pass", "old-hash"))
            .Returns(true);
        passwordHasherMock
            .Setup(x => x.HashPassword("new-pass"))
            .Returns("new-hash");

        var sessionA = RefreshTokenBuilder.Build(userId: user.Id);
        var sessionB = RefreshTokenBuilder.Build(userId: user.Id);

        var refreshTokenReadRepository = new RefreshTokenReadRepositoryBuilder()
            .SetupGetActiveTokensByUserId(user.Id, [sessionA, sessionB])
            .Build();

        var unitOfWorkMock = new Mock<DotCruz.CoreAuth.Domain.Interfaces.Data.IUnitOfWork>();

        var handler = new ChangePasswordCommandHandler(
            userWriteRepository,
            refreshTokenReadRepository,
            loggedUserMock.Object,
            passwordHasherMock.Object,
            unitOfWorkMock.Object
        );

        await handler.Handle(new ChangePasswordCommand("current-pass", "new-pass"), TestContext.Current.CancellationToken);

        sessionA.IsRevoked.Should().BeTrue();
        sessionB.IsRevoked.Should().BeTrue();
    }
}
