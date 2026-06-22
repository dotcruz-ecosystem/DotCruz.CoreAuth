using CommonTestUtilities.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Entities.Tokens;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;

namespace Domain.Test.Tokens
{
    public class PasswordResetTokenTest
    {
        [Fact]
        public void Success()
        {
            var passwordResetToken = PasswordResetTokenBuilder.Build();

            Assert.NotEmpty(passwordResetToken.Token);
            Assert.True(passwordResetToken.ExpiresAt > DateTimeOffset.UtcNow);
            Assert.False(passwordResetToken.IsUsed);
        }

        [Fact]
        public void Success_Mark_As_Used()
        {
            var passwordResetToken = PasswordResetTokenBuilder.Build();

            Assert.False(passwordResetToken.IsUsed);

            passwordResetToken.MarkAsUsed();

            Assert.True(passwordResetToken.IsUsed);
        }

        [Fact]
        public void Error_Token_Empty()
        {
            static void Action() => PasswordResetTokenBuilder.Build(token: string.Empty);

            var exception = Assert.Throws<ErrorOnValidationException>(Action);

            Assert.Contains(ResourceMessagesException.TOKEN_EMPTY, exception.GetErrorsMessages());
            Assert.Single(exception.GetErrorsMessages());
        }

        [Fact]
        public void Error_Expiration_Date_Earlier_Current_Date()
        {
            static void Action() => PasswordResetTokenBuilder.Build(expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

            var exception = Assert.Throws<ErrorOnValidationException>(Action);

            Assert.Contains(ResourceMessagesException.EXPIRATION_DATE_EARLIER_CURRENT_DATE, exception.GetErrorsMessages());
            Assert.Single(exception.GetErrorsMessages());
        }

        [Fact]
        public void Error_Mark_As_Used_When_Token_Expired()
        {
            var token = PasswordResetTokenBuilder.Build();

            var expiresAtProperty = typeof(PasswordResetToken).GetProperty("ExpiresAt");
            expiresAtProperty?.SetValue(token, DateTimeOffset.UtcNow.AddMinutes(-5));

            void Action() => token.MarkAsUsed();

            var exception = Assert.Throws<ErrorOnValidationException>(Action);

            Assert.Contains(ResourceMessagesException.TOKEN_EXPIRED, exception.GetErrorsMessages());
            Assert.Single(exception.GetErrorsMessages());
        }
    }
}
