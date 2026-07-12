using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DotCruz.CoreAuth.Infrastructure.Security.Tokens.Access.Validator
{
    public class JwtTokenValidator : JwtTokenHandler, IAccessTokenValidator
    {
        private readonly JwtTokenSettings _jwtTokenSettings;

        public JwtTokenValidator(IOptions<JwtTokenSettings> jwtTokenSettings)
        {
            _jwtTokenSettings = jwtTokenSettings.Value;
        }

        public Guid ValidateAndGetUserIdentifier(string token)
        {
            var validationParameter = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = _jwtTokenSettings.Audience,
                ValidateIssuer = true,
                ValidIssuer = _jwtTokenSettings.Issuer,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SecurityKey(_jwtTokenSettings.PrivateKeyPem, _jwtTokenSettings.Kid),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, validationParameter, out _);

            var userId = principal.Claims.First(c => c.Type == ClaimTypes.Sid).Value;

            return Guid.Parse(userId);
        }
    }
}
