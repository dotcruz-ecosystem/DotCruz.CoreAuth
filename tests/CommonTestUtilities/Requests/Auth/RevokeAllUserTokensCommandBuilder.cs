using DotCruz.CoreAuth.Application.Commands.Auth.RevokeAllUserTokens;

namespace CommonTestUtilities.Requests.Auth;

public class RevokeAllUserTokensCommandBuilder
{
    public static RevokeAllUserTokensCommand Build()
    {
        return new RevokeAllUserTokensCommand();
    }
}
