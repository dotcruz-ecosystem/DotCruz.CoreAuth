using System.Net;

namespace DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;

public class ConflictException : CoreAuthException
{
    private readonly IEnumerable<string> _errors;

    public ConflictException(string error) : base(error) => _errors = [error];
    public ConflictException(IEnumerable<string> errors) : base(string.Empty) => _errors = errors;

    public override IEnumerable<string> GetErrorsMessages() => _errors;
    public override HttpStatusCode GetStatusCode() => HttpStatusCode.Conflict;
}
