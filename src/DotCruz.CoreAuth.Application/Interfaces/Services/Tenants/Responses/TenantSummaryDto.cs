namespace DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;

public sealed record TenantSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    string Type,
    string Plan
);
