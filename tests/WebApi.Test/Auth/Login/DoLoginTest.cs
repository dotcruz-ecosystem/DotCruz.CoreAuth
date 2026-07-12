using DotCruz.CoreAuth.Application.Commands.Auth.Login;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Auth.Login;

public class DoLoginTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/auth/login";

    private readonly string _email;
    private readonly string _password;
    private readonly string _name;
    private readonly string _tenantId;

    public DoLoginTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _email = factory.GetEmail();
        _password = factory.GetPassword();
        _name = factory.GetName();
        _tenantId = factory.GetTenantId().ToString()!;
    }

    [Fact]
    public async Task Success()
    {
        var command = new LoginCommand(_email, _password);

        var response = await DoPost(method: METHOD, request: command, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var responseData = await JsonDocument.ParseAsync(responseBody, cancellationToken: TestContext.Current.CancellationToken);

        responseData.RootElement.GetProperty("name").GetString().Should().NotBeNullOrWhiteSpace().And.Be(_name);
        responseData.RootElement.GetProperty("email").GetString().Should().NotBeNullOrWhiteSpace().And.Be(_email.ToLowerInvariant());
        responseData.RootElement.GetProperty("tokens").GetProperty("access_token").GetString().Should().NotBeNullOrWhiteSpace();
        responseData.RootElement.GetProperty("tokens").GetProperty("refresh_token").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Login_Wrong_Password(string culture)
    {
        var command = new LoginCommand(_email, "wrongpassword");

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await using var responseBody = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var responseData = await JsonDocument.ParseAsync(responseBody, cancellationToken: TestContext.Current.CancellationToken);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_OR_PASSWORD_INVALID", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Login_Email_Not_Exists(string culture)
    {
        var command = new LoginCommand("emailnotexists@email.com", _password);

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        await using var responseBody = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var responseData = await JsonDocument.ParseAsync(responseBody, cancellationToken: TestContext.Current.CancellationToken);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_OR_PASSWORD_INVALID", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Login_Email_Empty(string culture)
    {
        var command = new LoginCommand(string.Empty, _password);

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var responseData = await JsonDocument.ParseAsync(responseBody, cancellationToken: TestContext.Current.CancellationToken);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_EMPTY", new CultureInfo(culture));

        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Login_Email_Invalid(string culture)
    {
        var command = new LoginCommand(_email.Replace('@', 'a'), _password);

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var responseData = await JsonDocument.ParseAsync(responseBody, cancellationToken: TestContext.Current.CancellationToken);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_INVALID", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Login_Password_Empty(string culture)
    {
        var command = new LoginCommand(_email, string.Empty);

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
        var responseData = await JsonDocument.ParseAsync(responseBody, cancellationToken: TestContext.Current.CancellationToken);

        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();

        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("PASSWORD_EMPTY", new CultureInfo(culture));

        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
