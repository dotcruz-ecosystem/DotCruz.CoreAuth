using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using FluentAssertions;
using System.Net;
using Xunit;

namespace WebApi.Test.Users;

public class CreateUserAuthorizationTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly string _userToken;
    private readonly string _superAdminToken;
    private readonly Guid _workspaceId;

    public CreateUserAuthorizationTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userToken = factory.GetUserAccessToken();
        _superAdminToken = factory.GetSuperAdminAccessToken();
        _workspaceId = factory.GetWorkspaceId().GetValueOrDefault();
    }

    [Fact]
    public async Task Error_Unauthenticated()
    {
        var command = CreateUserCommandBuilder.Build() with { Type = UserType.SuperAdmin };

        var response = await DoPost(method: METHOD, request: command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Error_Standard_User_Cannot_Create_Any_User()
    {
        var command = CreateUserCommandBuilder.Build() with { Type = UserType.User };

        var response = await DoPost(method: METHOD, request: command, token: _userToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData(UserType.SuperAdmin)]
    [InlineData(UserType.InternalSupport)]
    public async Task Error_Standard_User_Cannot_Escalate_To(UserType elevatedType)
    {
        var command = CreateUserCommandBuilder.Build() with { Type = elevatedType };

        var response = await DoPost(method: METHOD, request: command, token: _userToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Success_Super_Admin_Creates_Elevated_User()
    {
        var command = CreateUserCommandBuilder.Build() with { Type = UserType.InternalSupport };

        var response = await DoPost(method: METHOD, request: command, token: _superAdminToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
