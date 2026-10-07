using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;

namespace DotCruz.CoreAuth.Application.Commands.Auth.Login
{
    public sealed record ResponseLoginWorkspaceDto(
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
        ResponseLoginWorkspaceDto? Workspace = null,
        IReadOnlyCollection<UserWorkspaceDto>? Workspaces = null
    );
}

