using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;

namespace DotCruz.CoreAuth.Infrastructure.Security.Tokens.Refresh;

public class RefreshTokenGenerator(ITokenProvider tokenProvider) : IRefreshTokenGenerator
{
    public string Generate() => tokenProvider.Value();
}
