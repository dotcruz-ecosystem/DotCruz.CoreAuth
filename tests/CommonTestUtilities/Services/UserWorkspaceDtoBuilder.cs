using Bogus;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;

namespace CommonTestUtilities.Services;

public class UserWorkspaceDtoBuilder
{
    public static UserWorkspaceDto Build(
        Guid? tenantId = null,
        string? tenantName = null,
        string? tenantSlug = null,
        string tenantType = "Personal",
        string tenantStatus = "Active",
        string role = "Owner",
        string memberStatus = "Active",
        string plan = "Free")
    {
        var faker = new Faker();
        return new UserWorkspaceDto(
            tenantId ?? Guid.NewGuid(),
            tenantName ?? faker.Company.CompanyName(),
            tenantSlug ?? faker.Internet.DomainWord(),
            tenantType,
            tenantStatus,
            role,
            memberStatus,
            plan
        );
    }
}
