using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentValidation;

namespace DotCruz.CoreAuth.Application.Commands.Auth.SwitchWorkspace;

public class SwitchWorkspaceCommandValidator : AbstractValidator<SwitchWorkspaceCommand>
{
    public SwitchWorkspaceCommandValidator()
    {
        RuleFor(x => x.TargetWorkspaceId)
            .NotEmpty().WithMessage(ResourceMessagesException.ID_EMPTY);
    }
}
