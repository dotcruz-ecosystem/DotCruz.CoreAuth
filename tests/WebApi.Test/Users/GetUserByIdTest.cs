using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Users;

public class GetUserByIdTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly Guid _userId;
    private readonly string _email;
    private readonly string _name;
    private readonly string _token;
    private readonly Guid _tenantId;

    public GetUserByIdTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userId = factory.GetUserId();
        _email = factory.GetEmail();
        _name = factory.GetName();
        _token = factory.GetTenantUserAccessToken();
        _tenantId = factory.GetTenantId().GetValueOrDefault();
    }

    [Fact]
    public async Task Success()
    {
        var response = await DoGet(token: _token, method: $"{METHOD}/{_userId}", tenantId: _tenantId.ToString());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("id").GetGuid().Should().Be(_userId);
        responseData.RootElement.GetProperty("name").GetString().Should().Be(_name);
        responseData.RootElement.GetProperty("email").GetString().Should().Be(_email.ToLowerInvariant());
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_User_Not_Found(string culture)
    {
        var nonExistentId = Guid.NewGuid();

        var response = await DoGet(token: _token, method: $"{METHOD}/{nonExistentId}", culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("USER_NOT_FOUND", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
