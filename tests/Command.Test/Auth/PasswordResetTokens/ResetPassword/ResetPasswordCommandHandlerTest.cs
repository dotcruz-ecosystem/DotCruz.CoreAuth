using CommonTestUtilities.Commands.Users;
using CommonTestUtilities.Entities.Tokens;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Repositories.Tokens;
using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Moq;
using DotCruz.CoreAuth.Domain.Entities.Tokens;

namespace Command.Test.Auth.PasswordResetTokens.ResetPassword
{
    public class ResetPasswordCommandHandlerTest
    {
        [Fact]
        public async Task Success()
        {
            var command = ResetPasswordCommandBuilder.Build();
            var user = UserBuilder.Build();
            var token = PasswordResetTokenBuilder.Build(token: "hashed-token");
            
            // Link user to token via reflection if necessary, but here we can just ensure it exists
            typeof(DotCruz.CoreAuth.Domain.Entities.Tokens.PasswordResetToken)
                .GetProperty("User")?.SetValue(token, user);

            var tokenReadRepository = new PasswordResetTokenReadRepositoryBuilder()
                .SetupGetByToken("hashed-token", token)
                .Build();

            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(t => t.Hash(command.Token)).Returns("hashed-token");

            var handler = new ResetPasswordCommandHandlerBuilder()
                .SetTokenReadRepository(tokenReadRepository)
                .SetTokenProvider(tokenProvider.Object)
                .Build();

            await handler.Handle(command, TestContext.Current.CancellationToken);

            Assert.True(token.IsUsed);
        }

        [Fact]
        public async Task Error_Token_Invalid()
        {
            var command = ResetPasswordCommandBuilder.Build();

            var tokenReadRepository = new PasswordResetTokenReadRepositoryBuilder()
                .SetupGetByToken(It.IsAny<string>(), null)
                .Build();

            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(t => t.Hash(It.IsAny<string>())).Returns("hashed-token");

            var handler = new ResetPasswordCommandHandlerBuilder()
                .SetTokenReadRepository(tokenReadRepository)
                .SetTokenProvider(tokenProvider.Object)
                .Build();

            Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

            var exception = await Assert.ThrowsAsync<NotFoundException>(act);

            Assert.Equal(ResourceMessagesException.TOKEN_INVALID, exception.Message);
        }
    

        [Fact]
        public async Task Success_Revokes_Active_Refresh_Tokens()
        {
            var command = ResetPasswordCommandBuilder.Build();
            var user = UserBuilder.Build();
            var resetToken = PasswordResetTokenBuilder.Build(token: "hashed-token", userId: user.Id);
            typeof(PasswordResetToken).GetProperty(nameof(PasswordResetToken.User))?.SetValue(resetToken, user);

            var sessionA = RefreshTokenBuilder.Build(userId: user.Id);
            var sessionB = RefreshTokenBuilder.Build(userId: user.Id);

            var tokenReadRepository = new PasswordResetTokenReadRepositoryBuilder()
                .SetupGetByToken("hashed-token", resetToken)
                .Build();

            var refreshTokenReadRepository = new RefreshTokenReadRepositoryBuilder()
                .SetupGetActiveTokensByUserId(user.Id, [sessionA, sessionB])
                .Build();

            var tokenProvider = new Mock<ITokenProvider>();
            tokenProvider.Setup(t => t.Hash(command.Token)).Returns("hashed-token");

            var handler = new ResetPasswordCommandHandlerBuilder()
                .SetTokenReadRepository(tokenReadRepository)
                .SetRefreshTokenReadRepository(refreshTokenReadRepository)
                .SetTokenProvider(tokenProvider.Object)
                .Build();

            await handler.Handle(command, TestContext.Current.CancellationToken);

            Assert.True(sessionA.IsRevoked);
            Assert.True(sessionB.IsRevoked);
        }
    }
}
