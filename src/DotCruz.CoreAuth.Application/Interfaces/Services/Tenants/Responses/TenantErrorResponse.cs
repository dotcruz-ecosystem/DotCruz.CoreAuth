namespace DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;

public sealed record TenantErrorResponse(IEnumerable<string> Errors);
