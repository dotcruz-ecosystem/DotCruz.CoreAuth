using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace DotCruz.CoreAuth.Infrastructure.Security.Tokens.Access
{
    public abstract class JwtTokenHandler
    {
        protected static RsaSecurityKey SecurityKey(string privateKeyPem, string kid)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            return new RsaSecurityKey(rsa)
            {
                KeyId = kid
            };
        }
    }
}
