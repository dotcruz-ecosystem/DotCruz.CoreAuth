using DotCruz.CoreAuth.Application.Commands.Auth.ActivateAccount;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Auth.Activate;

public class ActivateAccountTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/auth/activate";

    private readonly string _pendingEmail;
    private readonly string _activationToken;
    private readonly string _tenantId;

    public ActivateAccountTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _pendingEmail = factory.GetPendingUserEmail();
        _activationToken = factory.GetActivationToken();
        _tenantId = factory.GetTenantId().ToString()!;
    }

    [Fact]
    public async Task Success()
    {
        var command = new ActivateAccountCommand(_activationToken, "NewStrongPassword123!");

        var response = await DoPost(method: METHOD, request: command, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify that the user is now active by attempting to login with new password
        var loginCommand = new DotCruz.CoreAuth.Application.Commands.Auth.Login.LoginCommand(_pendingEmail, "NewStrongPassword123!");
        var loginResponse = await DoPost(method: "api/auth/login", request: loginCommand, tenantId: _tenantId);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Token_Empty(string culture)
    {
        var command = new ActivateAccountCommand(string.Empty, "NewStrongPassword123!");

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
    public async Task Error_Password_Too_Short(string culture)
    {
        var command = new ActivateAccountCommand(_activationToken, "short");

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
        var command = new ActivateAccountCommand("invalid-token", "NewStrongPassword123!");

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("TOKEN_INVALID", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
