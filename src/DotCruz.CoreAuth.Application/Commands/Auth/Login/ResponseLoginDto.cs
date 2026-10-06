using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;

namespace DotCruz.CoreAuth.Application.Commands.Auth.Login
{
    public sealed record ResponseLoginTenantDto(
        Guid Id,
        string Name,
        string Slug,
        string? Type,
        string? Plan,
        string? Role = null
    );

    public record ResponseLoginDto(
        Guid Id,
        string Name,
        string Email,
        ResponseTokensDto Tokens,
        ResponseLoginTenantDto? Tenant = null,
        IReadOnlyCollection<UserWorkspaceDto>? Workspaces = null
    );
}

