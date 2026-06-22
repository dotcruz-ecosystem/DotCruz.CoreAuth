using CommonTestUtilities.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;

namespace Domain.Test.Tokens
{
    public class RefreshTokenTest
    {
        [Fact]
        public void Success_On_Create()
        {
            var refreshToken = RefreshTokenBuilder.Build();

            Assert.NotNull(refreshToken);
            Assert.NotEmpty(refreshToken.Token);
            Assert.False(refreshToken.IsExpired);
            Assert.False(refreshToken.IsRevoked);
            Assert.True(refreshToken.IsActive);
        }

        [Fact]
        public void Success_On_Revoke()
        {
            var refreshToken = RefreshTokenBuilder.Build();

            Assert.False(refreshToken.IsRevoked);

            refreshToken.Revoke();

            Assert.True(refreshToken.IsRevoked);
        }

        [Fact]
        public void Error_Token_Empty()
        {
            static void Action() =>  RefreshTokenBuilder.Build(token: string.Empty);

            var exception = Assert.Throws<ErrorOnValidationException>(Action);

            Assert.Contains(ResourceMessagesException.TOKEN_EMPTY, exception.GetErrorsMessages());
            Assert.Single(exception.GetErrorsMessages());
        }

        [Fact]
        public void Error_Expiration_Date_Earlier_Current_Date()
        {
            static void Action() => RefreshTokenBuilder.Build(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-5));

            var exception = Assert.Throws<ErrorOnValidationException>(Action);

            Assert.Contains(ResourceMessagesException.EXPIRATION_DATE_EARLIER_CURRENT_DATE, exception.GetErrorsMessages());
            Assert.Single(exception.GetErrorsMessages());
        }
    }
}
