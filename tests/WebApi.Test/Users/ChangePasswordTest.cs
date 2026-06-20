using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Application.Commands.Users.ChangePassword;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Users;

public class ChangePasswordTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly Guid _userId;
    private readonly string _email;
    private readonly string _password;
    private readonly Guid _tenantId;

    public ChangePasswordTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userId = factory.GetUserId();
        _email = factory.GetEmail();
        _password = factory.GetPassword();
        _tenantId = factory.GetTenantId().GetValueOrDefault();
    }

    [Fact]
    public async Task Success()
    {
        var request = new ChangePasswordRequest(
            CurrentPassword: _password,
            NewPassword: "NewStrongPassword123!"
        );

        var response = await DoPut(method: $"{METHOD}/{_userId}/change-password", request: request, tenantId: _tenantId.ToString());

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var loginCommand = LoginCommandBuilder.Build() with { Email = _email, Password = "NewStrongPassword123!" };
        var loginResponse = await DoPost(method: "api/auth/login", request: loginCommand);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Current_Password_Wrong(string culture)
    {
        var request = new ChangePasswordRequest(
            CurrentPassword: "wrongpassword",
            NewPassword: "NewStrongPassword123!"
        );

        var response = await DoPut(method: $"{METHOD}/{_userId}/change-password", request: request, culture: culture, tenantId: _tenantId.ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_OR_PASSWORD_INVALID", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Password_Empty(string culture)
    {
        var request = new ChangePasswordRequest(
            CurrentPassword: _password,
            NewPassword: string.Empty
        );

        var response = await DoPut(method: $"{METHOD}/{_userId}/change-password", request: request, culture: culture, tenantId: _tenantId.ToString());

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
        var request = new ChangePasswordRequest(
            CurrentPassword: _password,
            NewPassword: "short"
        );

        var response = await DoPut(method: $"{METHOD}/{_userId}/change-password", request: request, culture: culture, tenantId: _tenantId.ToString());

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
    public async Task Error_User_Not_Found(string culture)
    {
        var nonExistentId = Guid.NewGuid();
        var request = new ChangePasswordRequest(
            CurrentPassword: _password,
            NewPassword: "NewStrongPassword123!"
        );

        var response = await DoPut(method: $"{METHOD}/{nonExistentId}/change-password", request: request, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("USER_NOT_FOUND", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
