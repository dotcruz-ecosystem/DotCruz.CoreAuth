namespace DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;

public sealed record WorkspaceErrorResponse(IEnumerable<string> Errors);
