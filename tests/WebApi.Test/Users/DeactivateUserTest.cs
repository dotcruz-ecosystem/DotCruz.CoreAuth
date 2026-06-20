using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Users;

public class DeactivateUserTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly Guid _userId;
    private readonly Guid _tenantId;

    public DeactivateUserTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _userId = factory.GetUserId();
        _tenantId = factory.GetTenantId().GetValueOrDefault();
    }

    [Fact]
    public async Task Success()
    {
        var response = await DoDelete(method: $"{METHOD}/{_userId}", tenantId: _tenantId.ToString());

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await DoGet(method: $"{METHOD}/{_userId}", tenantId: _tenantId.ToString());
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_User_Not_Found(string culture)
    {
        var nonExistentId = Guid.NewGuid();

        var response = await DoDelete(method: $"{METHOD}/{nonExistentId}", culture: culture);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("USER_NOT_FOUND", new CultureInfo(culture));
        errors.Should().ContainSingle().And.Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
