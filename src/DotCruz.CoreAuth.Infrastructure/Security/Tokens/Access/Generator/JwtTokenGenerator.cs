using DotCruz.CoreAuth.Common.Settings;
using DotCruz.CoreAuth.Domain.Constants;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DotCruz.CoreAuth.Infrastructure.Security.Tokens.Access.Generator
{
    public class JwtTokenGenerator : JwtTokenHandler, IAccessTokenGenerator
    {
        private readonly JwtTokenSettings _jwtTokenSettings;

        public JwtTokenGenerator(IOptions<JwtTokenSettings> jwtTokenSettings)
        {
            _jwtTokenSettings = jwtTokenSettings.Value;
        }

        public string Generate(User user)
        {
            var claims = new List<Claim>()
            {
                new(ClaimTypes.Sid, user.Id.ToString()),
                new(ClaimTypes.Role, user.Type.ToString()),
                new(CustomClaimTypes.TenantId, user.TenantId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtTokenSettings.ExpirationTimeMinutes),
                Issuer = _jwtTokenSettings.Issuer,
                Audience = _jwtTokenSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    SecurityKey(_jwtTokenSettings.PrivateKeyPem, _jwtTokenSettings.Kid), 
                    SecurityAlgorithms.RsaSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(securityToken);
        }
    }
}
