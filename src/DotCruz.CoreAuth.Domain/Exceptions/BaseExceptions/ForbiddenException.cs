using System.Net;

namespace DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;

public class ForbiddenException(string message) : CoreAuthException(message)
{
    public override IEnumerable<string> GetErrorsMessages() => [Message];

    public override HttpStatusCode GetStatusCode() => HttpStatusCode.Forbidden;
}
