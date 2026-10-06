using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;
using MediatR;

namespace DotCruz.CoreAuth.Application.Queries.Auth.GetUserWorkspaces;

public sealed record GetUserWorkspacesQuery : IRequest<IReadOnlyCollection<UserWorkspaceDto>>;
