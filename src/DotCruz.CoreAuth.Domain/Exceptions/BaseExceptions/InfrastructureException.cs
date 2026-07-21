using System.Net;

namespace DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;

public class InfrastructureException : CoreAuthException
{
    private readonly IEnumerable<string> _errors;

    public InfrastructureException(string error) : base(error) => _errors = [error];
    public InfrastructureException(IEnumerable<string> errors) : base(string.Empty) => _errors = errors;

    public override IEnumerable<string> GetErrorsMessages() => _errors;
    public override HttpStatusCode GetStatusCode() => HttpStatusCode.InternalServerError;
}
