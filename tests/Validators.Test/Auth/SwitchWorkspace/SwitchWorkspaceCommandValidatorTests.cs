using CommonTestUtilities.Requests.Auth;
using DotCruz.CoreAuth.Application.Commands.Auth.SwitchWorkspace;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;

namespace Validators.Test.Auth.SwitchWorkspace;

public class SwitchWorkspaceCommandValidatorTests
{
    [Fact]
    public void Success()
    {
        var command = SwitchWorkspaceCommandBuilder.Build();

        var result = new SwitchWorkspaceCommandValidator().Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Error_TargetWorkspaceId_Empty()
    {
        var command = SwitchWorkspaceCommandBuilder.Build(isEmptyWorkspaceId: true);

        var result = new SwitchWorkspaceCommandValidator().Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ResourceMessagesException.ID_EMPTY, error.ErrorMessage);
    }
}
