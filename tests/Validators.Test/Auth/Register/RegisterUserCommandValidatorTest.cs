using CommonTestUtilities.Requests.Auth;
using DotCruz.CoreAuth.Application.Commands.Auth.Register;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using Xunit;

namespace Validators.Test.Auth.Register;

public class RegisterUserCommandValidatorTest
{
    [Fact]
    public void Success()
    {
        var validator = new RegisterUserCommandValidator();
        var request = RegisterUserCommandBuilder.Build();

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Error_Name_Empty()
    {
        var validator = new RegisterUserCommandValidator();
        var request = RegisterUserCommandBuilder.Build(name: string.Empty);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(ResourceMessagesException.NAME_EMPTY));
    }

    [Fact]
    public void Error_Email_Empty()
    {
        var validator = new RegisterUserCommandValidator();
        var request = RegisterUserCommandBuilder.Build(email: string.Empty);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(ResourceMessagesException.EMAIL_EMPTY));
    }

    [Fact]
    public void Error_Email_Invalid()
    {
        var validator = new RegisterUserCommandValidator();
        var request = RegisterUserCommandBuilder.Build(email: "email-invalido");

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains(ResourceMessagesException.EMAIL_INVALID));
    }
}
