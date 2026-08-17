using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Domain.Enums.Users;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Users;

public class CreateUserTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly string _email;
    private readonly string _token;
    private readonly string _tenantId;

    public CreateUserTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _email = factory.GetEmail();
        _token = factory.GetSuperAdminAccessToken();
        _tenantId = factory.GetTenantId().ToString()!;
    }

    [Fact]
    public async Task Success()
    {
        var command = CreateUserCommandBuilder.Build();

        var response = await DoPost(token: _token, method: METHOD, request: command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody = await response.Content.ReadAsStringAsync();
        var createdId = JsonSerializer.Deserialize<Guid>(responseBody);

        createdId.Should().NotBeEmpty();

        var getResponse = await DoGet(token: _token, method: $"api/users/{createdId}", tenantId: command.TenantId.ToString());
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponseBody = await getResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(getResponseBody);
        doc.RootElement.GetProperty("name").GetString().Should().Be(command.Name);
        doc.RootElement.GetProperty("email").GetString().Should().Be(command.Email.ToLowerInvariant());
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Name_Empty(string culture)
    {
        var command = CreateUserCommandBuilder.Build() with { Name = string.Empty };

        var response = await DoPost(token: _token, method: METHOD, request: command, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("NAME_EMPTY", new CultureInfo(culture));
        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Email_Empty(string culture)
    {
        var command = CreateUserCommandBuilder.Build() with { Email = string.Empty };

        var response = await DoPost(token: _token, method: METHOD, request: command, culture: culture);

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
        var command = CreateUserCommandBuilder.Build() with { Email = "invalid-email" };

        var response = await DoPost(token: _token, method: METHOD, request: command, culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_INVALID", new CultureInfo(culture));
        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Email_Already_Exists(string culture)
    {
        var command = CreateUserCommandBuilder.Build() with { Email = _email };

        var response = await DoPost(token: _token, method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_ALREADY_EXISTS", new CultureInfo(culture));
        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
