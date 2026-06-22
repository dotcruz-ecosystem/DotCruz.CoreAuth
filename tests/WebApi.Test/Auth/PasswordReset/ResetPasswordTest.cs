using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Auth.PasswordReset;

public class ResetPasswordTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/auth/password-reset/reset";

    private readonly string _email;
    private readonly string _resetToken;
    private readonly string _tenantId;

    public ResetPasswordTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _email = factory.GetEmail();
        _resetToken = factory.GetPasswordResetToken();
        _tenantId = factory.GetTenantId().ToString()!;
    }

    [Fact]
    public async Task Success()
    {
        var command = ResetPasswordCommandBuilder.Build() with { Token = _resetToken, NewPassword = "NewStrongPassword123!" };

        var response = await DoPost(method: METHOD, request: command, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var loginCommand = LoginCommandBuilder.Build() with { Email = _email, Password = "NewStrongPassword123!" };
        var loginResponse = await DoPost(method: "api/auth/login", request: loginCommand, tenantId: _tenantId);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Token_Empty(string culture)
    {
        var command = ResetPasswordCommandBuilder.Build() with { Token = string.Empty, NewPassword = "NewStrongPassword123!" };

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("TOKEN_EMPTY", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Password_Empty(string culture)
    {
        var command = ResetPasswordCommandBuilder.Build() with { Token = _resetToken, NewPassword = string.Empty };

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("PASSWORD_EMPTY", new CultureInfo(culture));
        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Password_Too_Short(string culture)
    {
        var command = ResetPasswordCommandBuilder.Build(passwordLength: 5) with { Token = _resetToken };

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = string.Format(
            ResourceMessagesException.ResourceManager.GetString("PASSWORD_MIN_LENGTH", new CultureInfo(culture))!, 8);
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Token_Invalid(string culture)
    {
        var command = ResetPasswordCommandBuilder.Build() with { Token = "invalid-token", NewPassword = "NewStrongPassword123!" };

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("TOKEN_INVALID", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
