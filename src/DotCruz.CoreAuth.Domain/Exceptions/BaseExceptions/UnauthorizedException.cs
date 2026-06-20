using System.Net;

namespace DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;

public class UnauthorizedException(string message) : CoreAuthException(message)
{
    public override IList<string> GetErrorsMessages() => [Message];

    public override HttpStatusCode GetStatusCode() => HttpStatusCode.Unauthorized;
}
