using CommonTestUtilities.Commands.Auth;
using CommonTestUtilities.Entities.Users;
using CommonTestUtilities.Requests.Auth;
using CommonTestUtilities.Security;
using CommonTestUtilities.Services;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;
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
        var targetWorkspaceId = Guid.NewGuid();

        var workspaceA = UserWorkspaceDtoBuilder.Build(workspaceId: Guid.NewGuid(), role: "Owner", memberStatus: "Active");
        var workspaceB = UserWorkspaceDtoBuilder.Build(workspaceId: targetWorkspaceId, workspaceType: "Dealership", role: "Admin", memberStatus: "Active", plan: "Pro");

        var loggedUser = new LoggedUserBuilder().SetupUser(user).Build();
        var workspaceServiceClient = new WorkspaceServiceClientBuilder()
            .SetupGetUserWorkspaces(user.Id, [workspaceA, workspaceB])
            .Build();

        var accessTokenGenerator = new Mock<IAccessTokenGenerator>();
        accessTokenGenerator
            .Setup(a => a.Generate(user, "Dealership", "Pro", targetWorkspaceId, "Admin"))
            .Returns("switched-access-token");

        var refreshTokenGenerator = new Mock<IRefreshTokenGenerator>();
        refreshTokenGenerator.Setup(r => r.Generate()).Returns("switched-refresh-token");

        var handler = new SwitchWorkspaceCommandHandlerBuilder()
            .SetLoggedUser(loggedUser)
            .SetWorkspaceServiceClient(workspaceServiceClient)
            .SetAccessTokenGenerator(accessTokenGenerator.Object)
            .SetRefreshTokenGenerator(refreshTokenGenerator.Object)
            .Build();

        var command = SwitchWorkspaceCommandBuilder.Build(targetWorkspaceId: targetWorkspaceId);
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal("switched-access-token", result.Tokens.AccessToken);
        Assert.Equal("switched-refresh-token", result.Tokens.RefreshToken);
        Assert.NotNull(result.Workspace);
        Assert.Equal(targetWorkspaceId, result.Workspace.Id);
        Assert.Equal("Dealership", result.Workspace.Type);
        Assert.Equal("Admin", result.Workspace.Role);
        Assert.Equal(2, result.Workspaces?.Count);
    }

    [Fact]
    public async Task Error_Workspace_Not_Found()
    {
        var user = UserBuilder.Build();
        var workspace = UserWorkspaceDtoBuilder.Build(workspaceId: Guid.NewGuid());

        var loggedUser = new LoggedUserBuilder().SetupUser(user).Build();
        var workspaceServiceClient = new WorkspaceServiceClientBuilder()
            .SetupGetUserWorkspaces(user.Id, [workspace])
            .Build();

        var handler = new SwitchWorkspaceCommandHandlerBuilder()
            .SetLoggedUser(loggedUser)
            .SetWorkspaceServiceClient(workspaceServiceClient)
            .Build();

        var unknownWorkspaceId = Guid.NewGuid();
        var command = SwitchWorkspaceCommandBuilder.Build(targetWorkspaceId: unknownWorkspaceId);

        Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<NotFoundException>(act);
        Assert.Contains(ResourceMessagesException.WORKSPACE_NOT_FOUND, exception.GetErrorsMessages());
    }

    [Fact]
    public async Task Error_Workspace_Member_Not_Active()
    {
        var user = UserBuilder.Build();
        var inactiveWorkspaceId = Guid.NewGuid();
        var workspace = UserWorkspaceDtoBuilder.Build(
            workspaceId: inactiveWorkspaceId, 
            memberStatus: "Invited");

        var loggedUser = new LoggedUserBuilder().SetupUser(user).Build();
        var workspaceServiceClient = new WorkspaceServiceClientBuilder()
            .SetupGetUserWorkspaces(user.Id, [workspace])
            .Build();

        var handler = new SwitchWorkspaceCommandHandlerBuilder()
            .SetLoggedUser(loggedUser)
            .SetWorkspaceServiceClient(workspaceServiceClient)
            .Build();

        var command = SwitchWorkspaceCommandBuilder.Build(targetWorkspaceId: inactiveWorkspaceId);

        Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<ErrorOnValidationException>(act);
        Assert.Contains(ResourceMessagesException.WORKSPACE_NOT_MEMBER_OR_INACTIVE, exception.GetErrorsMessages());
    }
}
