using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Xunit;

namespace WebApi.Test.Auth.RevokeTokens;

public class RevokeTokensTest : DotCruzCoreAuthClassFixture
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly string _refreshToken;
    private readonly string _token;

    public RevokeTokensTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
        _refreshToken = factory.GetRefreshToken();
        _token = factory.GetAccessToken();
    }

    [Fact]
    public async Task Success()
    {
        var response = await DoPost(method: "api/auth/revoke-tokens", request: new { }, token: _token, tenantId: _factory.GetTenantId().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshCommand = CommonTestUtilities.Requests.Auth.RefreshTokenCommandBuilder.Build(_refreshToken);
        var refreshResponse = await DoPost(method: "api/auth/refresh-token", request: refreshCommand);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Success_No_Tokens()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DotCruz.CoreAuth.Infrastructure.Data.CoreAuthDbContext>();

        var newUser = CommonTestUtilities.Entities.Users.UserBuilder.Build();
        dbContext.Users.Add(newUser);
        await dbContext.SaveChangesAsync();

        var fakeToken = _factory.GenerateAccessToken(newUser);

        var response = await DoPost(method: "api/auth/revoke-tokens", request: new { }, token: fakeToken, tenantId: newUser.TenantId.ToString());

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
