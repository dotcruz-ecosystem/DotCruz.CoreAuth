using Bogus;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;

namespace CommonTestUtilities.Services;

public class UserWorkspaceDtoBuilder
{
    public static UserWorkspaceDto Build(
        Guid? workspaceId = null,
        string? workspaceName = null,
        string? workspaceSlug = null,
        string workspaceType = "Personal",
        string workspaceStatus = "Active",
        string role = "Owner",
        string memberStatus = "Active",
        string plan = "Free")
    {
        var faker = new Faker();
        return new UserWorkspaceDto(
            workspaceId ?? Guid.NewGuid(),
            workspaceName ?? faker.Company.CompanyName(),
            workspaceSlug ?? faker.Internet.DomainWord(),
            workspaceType,
            workspaceStatus,
            role,
            memberStatus,
            plan
        );
    }
}
