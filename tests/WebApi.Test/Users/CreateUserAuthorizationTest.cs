using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using FluentAssertions;
using System.Net;
using Xunit;

namespace WebApi.Test.Users;

public class CreateUserAuthorizationTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly string _tenantUserToken;
    private readonly string _superAdminToken;
    private readonly Guid _tenantId;

    public CreateUserAuthorizationTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _tenantUserToken = factory.GetTenantUserAccessToken();
        _superAdminToken = factory.GetSuperAdminAccessToken();
        _tenantId = factory.GetTenantId().GetValueOrDefault();
    }

    [Fact]
    public async Task Error_Unauthenticated()
    {
        var command = CreateUserCommandBuilder.Build() with { Type = UserType.SuperAdmin };

        var response = await DoPost(method: METHOD, request: command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Error_Tenant_User_Cannot_Create_Any_User()
    {
        var command = CreateUserCommandBuilder.Build() with { Type = UserType.TenantUser, TenantId = _tenantId };

        var response = await DoPost(method: METHOD, request: command, token: _tenantUserToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(UserType.SuperAdmin)]
    [InlineData(UserType.InternalSupport)]
    public async Task Error_Tenant_User_Cannot_Escalate_To(UserType elevatedType)
    {
        var command = CreateUserCommandBuilder.Build() with { Type = elevatedType, TenantId = _tenantId };

        var response = await DoPost(method: METHOD, request: command, token: _tenantUserToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Success_Super_Admin_Creates_Elevated_User()
    {
        var command = CreateUserCommandBuilder.Build() with { Type = UserType.TenantAdmin, TenantId = _tenantId };

        var response = await DoPost(method: METHOD, request: command, token: _superAdminToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
