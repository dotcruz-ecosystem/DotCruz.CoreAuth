using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Repositories.Users;
using DotCruz.CoreAuth.Application.Commands.Users.DeactivateUser;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Tokens;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Command.Test.Users.DeactivateUser;

public class DeactivateUserCommandHandlerTest
{
    [Fact]
    public async Task Success()
    {
        var user = UserBuilder.Build();
        var userWriteRepository = new UserWriteRepositoryBuilder()
            .SetupGetByIdToUpdate(user)
            .Build();

        var token1 = new RefreshToken("token-1", DateTimeOffset.UtcNow.AddDays(1), user.Id);
        var token2 = new RefreshToken("token-2", DateTimeOffset.UtcNow.AddDays(1), user.Id);
        var activeTokens = new List<RefreshToken> { token1, token2 };

        var refreshTokenReadRepositoryMock = new Mock<IRefreshTokenReadRepository>();
        refreshTokenReadRepositoryMock
            .Setup(x => x.GetActiveTokensByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeTokens);

        var unitOfWorkMock = new Mock<DotCruz.CoreAuth.Domain.Interfaces.Data.IUnitOfWork>();

        var handler = new DeactivateUserCommandHandler(
            userWriteRepository,
            unitOfWorkMock.Object
        );

        var command = new DeactivateUserCommand(user.Id);

        await handler.Handle(command, CancellationToken.None);

        user.DeletedAt.Should().NotBeNull();
        token1.IsRevoked.Should().BeFalse();
        token2.IsRevoked.Should().BeFalse();
        unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Error_User_NotFound()
    {
        var userId = Guid.NewGuid();
        var userWriteRepository = new UserWriteRepositoryBuilder()
            .SetupGetByIdToUpdate(userId, null)
            .Build();

        var refreshTokenReadRepositoryMock = new Mock<IRefreshTokenReadRepository>();
        var unitOfWorkMock = new Mock<DotCruz.CoreAuth.Domain.Interfaces.Data.IUnitOfWork>();

        var handler = new DeactivateUserCommandHandler(
            userWriteRepository,
            unitOfWorkMock.Object
        );

        var command = new DeactivateUserCommand(userId);

        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.And.Message.Should().Be(ResourceMessagesException.USER_NOT_FOUND);
    }
}
