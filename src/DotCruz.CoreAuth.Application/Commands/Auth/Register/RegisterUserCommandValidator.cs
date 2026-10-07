using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentValidation;

namespace DotCruz.CoreAuth.Application.Commands.Auth.Register;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ResourceMessagesException.NAME_EMPTY)
            .MaximumLength(200).WithMessage(string.Format(ResourceMessagesException.NAME_MAX_LENGTH, 200));

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ResourceMessagesException.EMAIL_EMPTY)
            .MaximumLength(200).WithMessage(string.Format(ResourceMessagesException.EMAIL_MAX_LENGTH, 200))
            .EmailAddress().WithMessage(ResourceMessagesException.EMAIL_INVALID);
    }
}
