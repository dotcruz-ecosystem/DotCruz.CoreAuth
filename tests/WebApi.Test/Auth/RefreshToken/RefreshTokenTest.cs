using DotCruz.CoreAuth.Application.Commands.Auth.RefreshTokens;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using FluentAssertions;
using System.Globalization;
using System.Net;
using System.Text.Json;
using WebApi.Test.InlineData;
using Xunit;

namespace WebApi.Test.Auth.RefreshToken;

public class RefreshTokenTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/auth/refresh-token";

    private readonly string _refreshToken;
    private readonly string _tenantId;

    public RefreshTokenTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _refreshToken = factory.GetRefreshToken();
        _tenantId = factory.GetTenantId().ToString()!;
    }

    [Fact]
    public async Task Success()
    {
        var command = new RefreshTokenCommand(_refreshToken);

        var response = await DoPost(method: METHOD, request: command, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("access_token").GetString().Should().NotBeNullOrWhiteSpace();
        responseData.RootElement.GetProperty("refresh_token").GetString().Should().NotBeNullOrWhiteSpace().And.NotBe(_refreshToken);
    }

    [Theory]
    [ClassData(typeof(CultureInlineDataTest))]
    public async Task Error_Token_Invalid(string culture)
    {
        var command = new RefreshTokenCommand("invalid-refresh-token");

        var response = await DoPost(method: METHOD, request: command, culture: culture, tenantId: _tenantId);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);
        var errors = responseData.RootElement.GetProperty("errors").EnumerateArray();
        var expectedMessage = ResourceMessagesException.ResourceManager.GetString("TOKEN_INVALID", new CultureInfo(culture));
        errors.Should().Contain(error => error.GetString()!.Equals(expectedMessage));
    }
}
