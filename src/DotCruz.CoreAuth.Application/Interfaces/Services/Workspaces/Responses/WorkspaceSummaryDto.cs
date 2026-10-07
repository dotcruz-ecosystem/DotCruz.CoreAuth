namespace DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;

public sealed record WorkspaceSummaryDto(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    string Type,
    string Plan
);
