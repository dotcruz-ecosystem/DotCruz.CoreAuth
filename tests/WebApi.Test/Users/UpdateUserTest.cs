using CommonTestUtilities.Requests.Users;
using DotCruz.CoreAuth.Application.Commands.Users.UpdateUser;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Users;

public class UpdateUserTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly Guid _userId;
    private readonly string _email;
    private readonly string _name;
    private readonly Guid? _tenantId;
    private readonly string _otherEmail;
    private readonly string _token;
    private readonly string _tenantUserToken;
    private readonly string _foreignTenantToken;

    public UpdateUserTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userId = factory.GetUserId();
        _email = factory.GetEmail();
        _name = factory.GetName();
        _tenantId = factory.GetTenantId();
        _otherEmail = factory.GetPendingUserEmail();
        _token = factory.GetSuperAdminAccessToken();
        _tenantUserToken = factory.GetTenantUserAccessToken();
        _foreignTenantToken = factory.GenerateSuperAdminTokenForOtherTenant();
    }

    [Fact]
    public async Task Success()
    {
        var request = UpdateUserCommandBuilder.Build().Request with { Name = "Updated Name", Email = "updatedemail@example.com" };

        var response = await DoPut(token: _token, 
            method: $"{METHOD}/{_userId}", 
            request: request, 
            tenantId: _tenantId?.ToString() ?? string.Empty
        );

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await DoGet(token: _token, 
            method: $"{METHOD}/{_userId}", 
            tenantId: _tenantId?.ToString() ?? string.Empty
        );
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponseBody = await getResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(getResponseBody);
        doc.RootElement.GetProperty("name").GetString().Should().Be(request.Name);
        doc.RootElement.GetProperty("email").GetString().Should().Be(request.Email.ToLowerInvariant());
    }

    [Fact]
    public async Task Error_Tenant_Isolation_Mismatch()
    {
        var request = UpdateUserCommandBuilder.Build().Request with { Name = "Updated Name", Email = "updatedemail@example.com" };

        var response = await DoPut(
            token: _foreignTenantToken,
            method: $"{METHOD}/{_userId}",
            request: request
        );

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Error_Unauthenticated()
    {
        var request = UpdateUserCommandBuilder.Build().Request with { Name = "Updated Name", Email = "updatedemail@example.com" };

        var response = await DoPut(method: $"{METHOD}/{_userId}", request: request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Error_Forbidden_For_Tenant_User()
    {
        var request = UpdateUserCommandBuilder.Build().Request with { Name = "Updated Name", Email = "updatedemail@example.com" };

        var response = await DoPut(token: _tenantUserToken, method: $"{METHOD}/{_userId}", request: request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Name_Empty(string culture)
    {
        var request = UpdateUserCommandBuilder.Build().Request with { Name = string.Empty, Email = _email };

        var response = await DoPut(token: _token, 
            method: $"{METHOD}/{_userId}", 
            request: request, 
            culture: culture,
            tenantId: _tenantId?.ToString() ?? string.Empty
        );

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
        var request = UpdateUserCommandBuilder.Build().Request with { Name = _name, Email = string.Empty };

        var response = await DoPut(token: _token, 
            method: $"{METHOD}/{_userId}", 
            request: request, 
            culture: culture,
            tenantId: _tenantId?.ToString() ?? string.Empty
        );

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
        var request = UpdateUserCommandBuilder.Build().Request with { Name = _name, Email = "invalid-email-format" };

        var response = await DoPut(token: _token, 
            method: $"{METHOD}/{_userId}", 
            request: request, 
            culture: culture,
            tenantId: _tenantId?.ToString() ?? string.Empty
        );

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
        var request = UpdateUserCommandBuilder.Build().Request with { Name = _name, Email = _otherEmail };

        var response = await DoPut(token: _token, 
            method: $"{METHOD}/{_userId}", 
            request: request, 
            culture: culture,
            tenantId: _tenantId?.ToString() ?? string.Empty
        );

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("EMAIL_ALREADY_EXISTS", new CultureInfo(culture));
        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
