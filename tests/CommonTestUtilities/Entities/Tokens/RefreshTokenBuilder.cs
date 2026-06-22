using Bogus;
using DotCruz.CoreAuth.Domain.Entities.Tokens;

namespace CommonTestUtilities.Entities.Tokens
{
    public class RefreshTokenBuilder
    {
        public static RefreshToken Build(string? token = null, DateTimeOffset? expiresAt = null, Guid? userId = null)
        {
            var refreshTokenFaker = new Faker<RefreshToken>()
                .CustomInstantiator(f => new RefreshToken(
                        token ?? f.Random.Guid().ToString(),
                        expiresAt ?? (DateTimeOffset)f.Date.Between(DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(30)),
                        userId ?? f.Random.Guid()
                    )
                );

            return refreshTokenFaker.Generate();
        }
    }
}
