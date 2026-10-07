using CommonTestUtilities.Entities.Users;
using DotCruz.CoreAuth.Application.Queries.Users.GetUserById;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Repositories.Users;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Command.Test.Users.GetUserById;

public class GetUserByIdQueryHandlerTest
{
    [Fact]
    public async Task Success()
    {
        var user = UserBuilder.Build();
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        userReadRepositoryMock
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new GetUserByIdQueryHandler(userReadRepositoryMock.Object);
        var query = new GetUserByIdQuery(user.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
        result.Type.Should().Be(user.Type);
        result.Status.Should().Be(user.Status);
        result.WorkspaceId.Should().Be(user.WorkspaceId);
    }

    [Fact]
    public async Task Error_User_NotFound()
    {
        var userId = Guid.NewGuid();
        var userReadRepositoryMock = new Mock<IUserReadRepository>();
        userReadRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DotCruz.CoreAuth.Domain.Entities.Users.User?)null);

        var handler = new GetUserByIdQueryHandler(userReadRepositoryMock.Object);
        var query = new GetUserByIdQuery(userId);

        Func<Task> act = () => handler.Handle(query, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.And.Message.Should().Be(ResourceMessagesException.USER_NOT_FOUND);
    }
}
