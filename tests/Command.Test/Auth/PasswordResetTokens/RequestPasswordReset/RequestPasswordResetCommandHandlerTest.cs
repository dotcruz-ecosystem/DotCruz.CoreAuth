using CommonTestUtilities.Commands.Users;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Repositories.Users;
using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Application.Interfaces.Services.Notification;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Moq;

namespace Command.Test.Auth.PasswordResetTokens.RequestPasswordReset
{
    public class RequestPasswordResetCommandHandlerTest
    {
        [Fact]
        public async Task Success()
        {
            var command = RequestPasswordResetCommandBuilder.Build();
            var user = UserBuilder.Build(email: command.Email);

            var userReadRepository = new UserReadRepositoryBuilder()
                .SetupGetUserByEmail(user)
                .Build();

            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(t => t.Value()).Returns("plain-token");
            tokenProvider.Setup(t => t.Hash("plain-token")).Returns("hashed-token");

            var emailService = new Mock<IEmailService>();

            var handler = new RequestPasswordResetCommandHandlerBuilder()
                .SetUserReadRepository(userReadRepository)
                .SetTokenProvider(tokenProvider.Object)
                .SetEmailService(emailService.Object)
                .Build();

            await handler.Handle(command, TestContext.Current.CancellationToken);

            emailService.Verify(e => e.SendPasswordResetEmailAsync(user.Email, user.Name, "plain-token", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Success_User_Not_Found()
        {
            var command = RequestPasswordResetCommandBuilder.Build();

            var userReadRepository = new UserReadRepositoryBuilder()
                .SetupGetUserByEmail(null)
                .Build();

            var emailService = new Mock<IEmailService>();

            var handler = new RequestPasswordResetCommandHandlerBuilder()
                .SetUserReadRepository(userReadRepository)
                .SetEmailService(emailService.Object)
                .Build();

            await handler.Handle(command, TestContext.Current.CancellationToken);

            emailService.Verify(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
