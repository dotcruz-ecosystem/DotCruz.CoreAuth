using Bogus;
using DotCruz.CoreAuth.Domain.Entities.Tokens;

namespace CommonTestUtilities.Entities.Tokens
{
    public class PasswordResetTokenBuilder
    {
        public static PasswordResetToken Build(string? token = null, DateTimeOffset? expiresAt = null, Guid? userId = null)
        {
            var passwordResetTokenFaker = new Faker<PasswordResetToken>()
                .CustomInstantiator(f => new PasswordResetToken(
                        token ?? f.Random.Guid().ToString(),
                        expiresAt ?? (DateTimeOffset)f.Date.Between(DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(30)),
                        userId ?? f.Random.Guid()
                    )
                );

            return passwordResetTokenFaker.Generate();
        }
    }
}
