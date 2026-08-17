using CommonTestUtilities.Commands.Users;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Requests.Users;
using CommonTestUtilities.Security;
using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Moq;

namespace Command.Test.Auth.Login
{
    public class LoginCommandHandlerTest
    {
        [Fact]
        public async Task Success()
        {
            var command = LoginCommandBuilder.Build();
            var user = UserBuilder.Build(email: command.Email);

            var userReadRepository = new UserReadRepositoryBuilder()
                .SetupGetUserByEmail(user)
                .Build();

            var passwordHasher = new PasswordHasherBuilder()
                .SetupVerifyPassword(true)
                .Build();

            var accessTokenGenerator = new Mock<IAccessTokenGenerator>();
            accessTokenGenerator.Setup(a => a.Generate(user)).Returns("access-token");

            var refreshTokenGenerator = new Mock<IRefreshTokenGenerator>();
            refreshTokenGenerator.Setup(r => r.Generate()).Returns("refresh-token");

            var handler = new LoginCommandHandlerBuilder()
                .SetUserReadRepository(userReadRepository)
                .SetPasswordHasher(passwordHasher)
                .SetAccessTokenGenerator(accessTokenGenerator.Object)
                .SetRefreshTokenGenerator(refreshTokenGenerator.Object)
                .Build();

            var result = await handler.Handle(command, TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Name, result.Name);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal("access-token", result.Tokens.AccessToken);
            Assert.Equal("refresh-token", result.Tokens.RefreshToken);
        }

        [Fact]
        public async Task Error_User_Not_Found()
        {
            var command = LoginCommandBuilder.Build();

            var userReadRepository = new UserReadRepositoryBuilder()
                .SetupGetUserByEmail(null)
                .Build();

            var handler = new LoginCommandHandlerBuilder()
                .SetUserReadRepository(userReadRepository)
                .Build();

            Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

            var exception = await Assert.ThrowsAsync<InvalidLoginException>(act);

            Assert.Contains(ResourceMessagesException.EMAIL_OR_PASSWORD_INVALID, exception.GetErrorsMessages());
        }

        [Fact]
        public async Task Error_Deactivated_User()
        {
            var command = LoginCommandBuilder.Build();
            var user = UserBuilder.Build(email: command.Email);
            user.Delete();

            Task act() => BuildHandlerWithValidPassword(user).Handle(command, TestContext.Current.CancellationToken);

            var exception = await Assert.ThrowsAsync<InvalidLoginException>(act);

            Assert.Contains(ResourceMessagesException.EMAIL_OR_PASSWORD_INVALID, exception.GetErrorsMessages());
        }

        [Fact]
        public async Task Error_Pending_Activation_User()
        {
            var command = LoginCommandBuilder.Build();
            var user = UserBuilder.Build(email: command.Email, status: UserStatus.PendingActivation);

            Task act() => BuildHandlerWithValidPassword(user).Handle(command, TestContext.Current.CancellationToken);

            var exception = await Assert.ThrowsAsync<InvalidLoginException>(act);

            Assert.Contains(ResourceMessagesException.EMAIL_OR_PASSWORD_INVALID, exception.GetErrorsMessages());
        }

        [Fact]
        public async Task Error_Blocked_User()
        {
            var command = LoginCommandBuilder.Build();
            var user = UserBuilder.Build(email: command.Email, status: UserStatus.Blocked);

            Task act() => BuildHandlerWithValidPassword(user).Handle(command, TestContext.Current.CancellationToken);

            var exception = await Assert.ThrowsAsync<InvalidLoginException>(act);

            Assert.Contains(ResourceMessagesException.EMAIL_OR_PASSWORD_INVALID, exception.GetErrorsMessages());
        }

        [Fact]
        public async Task Error_Invalid_Password()
        {
            var command = LoginCommandBuilder.Build();
            var user = UserBuilder.Build(email: command.Email);

            var userReadRepository = new UserReadRepositoryBuilder()
                .SetupGetUserByEmail(user)
                .Build();

            var passwordHasher = new PasswordHasherBuilder()
                .SetupVerifyPassword(false)
                .Build();

            var handler = new LoginCommandHandlerBuilder()
                .SetUserReadRepository(userReadRepository)
                .SetPasswordHasher(passwordHasher)
                .Build();

            Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

            var exception = await Assert.ThrowsAsync<InvalidLoginException>(act);

            Assert.Contains(ResourceMessagesException.EMAIL_OR_PASSWORD_INVALID, exception.GetErrorsMessages());
        }

        private static LoginCommandHandler BuildHandlerWithValidPassword(User user)
        {
            var userReadRepository = new UserReadRepositoryBuilder()
                .SetupGetUserByEmail(user)
                .Build();

            var passwordHasher = new PasswordHasherBuilder()
                .SetupVerifyPassword(true)
                .Build();

            return new LoginCommandHandlerBuilder()
                .SetUserReadRepository(userReadRepository)
                .SetPasswordHasher(passwordHasher)
                .Build();
        }
    }
}
