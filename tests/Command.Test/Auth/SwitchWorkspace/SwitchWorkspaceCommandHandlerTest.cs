using CommonTestUtilities.Commands.Auth;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Requests.Auth;
using CommonTestUtilities.Security;
using CommonTestUtilities.Services;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;
using DotCruz.CoreAuth.Domain.Entities.Users;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using DotCruz.CoreAuth.Domain.Interfaces.Security.Tokens;
using Moq;

namespace Command.Test.Auth.SwitchWorkspace;

public class SwitchWorkspaceCommandHandlerTest
{
    [Fact]
    public async Task Success()
    {
        var user = UserBuilder.Build();
        var targetTenantId = Guid.NewGuid();

        var workspaceA = UserWorkspaceDtoBuilder.Build(tenantId: user.TenantId, role: "Owner", memberStatus: "Active");
        var workspaceB = UserWorkspaceDtoBuilder.Build(tenantId: targetTenantId, tenantType: "Dealership", role: "Admin", memberStatus: "Active", plan: "Pro");

        var loggedUser = new LoggedUserBuilder().SetupUser(user).Build();
        var tenantServiceClient = new TenantServiceClientBuilder()
            .SetupGetUserWorkspaces(user.Id, [workspaceA, workspaceB])
            .Build();

        var accessTokenGenerator = new Mock<IAccessTokenGenerator>();
        accessTokenGenerator
            .Setup(a => a.Generate(user, "Dealership", "Pro", targetTenantId, "Admin"))
            .Returns("switched-access-token");

        var refreshTokenGenerator = new Mock<IRefreshTokenGenerator>();
        refreshTokenGenerator.Setup(r => r.Generate()).Returns("switched-refresh-token");

        var handler = new SwitchWorkspaceCommandHandlerBuilder()
            .SetLoggedUser(loggedUser)
            .SetTenantServiceClient(tenantServiceClient)
            .SetAccessTokenGenerator(accessTokenGenerator.Object)
            .SetRefreshTokenGenerator(refreshTokenGenerator.Object)
            .Build();

        var command = SwitchWorkspaceCommandBuilder.Build(targetTenantId: targetTenantId);
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("switched-access-token", result.Tokens.AccessToken);
        Assert.Equal("switched-refresh-token", result.Tokens.RefreshToken);
        Assert.NotNull(result.Tenant);
        Assert.Equal(targetTenantId, result.Tenant.Id);
        Assert.Equal("Dealership", result.Tenant.Type);
        Assert.Equal("Admin", result.Tenant.Role);
        Assert.Equal(2, result.Workspaces?.Count);
    }

    [Fact]
    public async Task Error_Workspace_Not_Found()
    {
        var user = UserBuilder.Build();
        var workspace = UserWorkspaceDtoBuilder.Build(tenantId: user.TenantId);

        var loggedUser = new LoggedUserBuilder().SetupUser(user).Build();
        var tenantServiceClient = new TenantServiceClientBuilder()
            .SetupGetUserWorkspaces(user.Id, [workspace])
            .Build();

        var handler = new SwitchWorkspaceCommandHandlerBuilder()
            .SetLoggedUser(loggedUser)
            .SetTenantServiceClient(tenantServiceClient)
            .Build();

        var unknownTenantId = Guid.NewGuid();
        var command = SwitchWorkspaceCommandBuilder.Build(targetTenantId: unknownTenantId);

        Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Contains(ResourceMessagesException.WORKSPACE_NOT_FOUND, exception.GetErrorsMessages());
    }

    [Fact]
    public async Task Error_Workspace_Member_Not_Active()
    {
        var user = UserBuilder.Build();
        var inactiveTenantId = Guid.NewGuid();
        var workspace = UserWorkspaceDtoBuilder.Build(
            tenantId: inactiveTenantId, 
            memberStatus: "Invited");

        var loggedUser = new LoggedUserBuilder().SetupUser(user).Build();
        var tenantServiceClient = new TenantServiceClientBuilder()
            .SetupGetUserWorkspaces(user.Id, [workspace])
            .Build();

        var handler = new SwitchWorkspaceCommandHandlerBuilder()
            .SetLoggedUser(loggedUser)
            .SetTenantServiceClient(tenantServiceClient)
            .Build();

        var command = SwitchWorkspaceCommandBuilder.Build(targetTenantId: inactiveTenantId);

        Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<ErrorOnValidationException>(act);
        Assert.Contains(ResourceMessagesException.WORKSPACE_NOT_MEMBER_OR_INACTIVE, exception.GetErrorsMessages());
    }
}
