using CommonTestUtilities.Commands.Auth.Register;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Requests.Auth;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Command.Test.Auth.Register;

public class RegisterUserCommandHandlerTest
{
    [Fact]
    public async Task Success()
    {
        var command = RegisterUserCommandBuilder.Build();

        var userReadRepository = new UserReadRepositoryBuilder()
            .SetupExistsActiveUserWithEmail(command.Email.ToLowerInvariant(), false)
            .Build();

        var handler = new RegisterUserCommandHandlerBuilder()
            .SetUserReadRepository(userReadRepository)
            .Build();

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Error_Email_Already_Exists()
    {
        var command = RegisterUserCommandBuilder.Build();

        var userReadRepository = new UserReadRepositoryBuilder()
            .SetupExistsActiveUserWithEmail(command.Email.ToLowerInvariant(), true)
            .Build();

        var handler = new RegisterUserCommandHandlerBuilder()
            .SetUserReadRepository(userReadRepository)
            .Build();

        Func<Task> act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await act.Should().ThrowAsync<ErrorOnValidationException>();
        exception.And.GetErrorsMessages().Should().Contain(ResourceMessagesException.EMAIL_ALREADY_EXISTS);
    }
}
