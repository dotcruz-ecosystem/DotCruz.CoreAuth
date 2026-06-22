using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Auth.PasswordReset;

public class RequestPasswordResetTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/auth/password-reset/request";

    private readonly string _email;
    private readonly string _tenantId;

    public RequestPasswordResetTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _email = factory.GetEmail();
        _tenantId = factory.GetTenantId().ToString()!;
    }

    [Fact]
    public async Task Success()
    {
        var command = RequestPasswordResetCommandBuilder.Build() with { Email = _email };

        var response = await DoPost(method: METHOD, request: command, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Success_Email_Not_Exists()
    {
        var command = RequestPasswordResetCommandBuilder.Build() with { Email = "notexists@example.com" };

        var response = await DoPost(method: METHOD, request: command, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Email_Empty(string culture)
    {
        var command = RequestPasswordResetCommandBuilder.Build() with { Email = string.Empty };

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_EMPTY", new CultureInfo(culture));
        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Email_Invalid(string culture)
    {
        var command = RequestPasswordResetCommandBuilder.Build() with { Email = "invalid-email" };

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_INVALID", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
