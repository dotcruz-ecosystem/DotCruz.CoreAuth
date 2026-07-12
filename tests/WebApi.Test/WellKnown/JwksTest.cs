using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace WebApi.Test.WellKnown;

public class JwksTest : DotCruzCoreAuthClassFixture
{
    public JwksTest(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_ShouldReturnJwks_Success()
    {
        // Act
        var response = await DoGet(".well-known/jwks.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        content.TryGetProperty("keys", out var keys).Should().BeTrue();
        keys.ValueKind.Should().Be(JsonValueKind.Array);
        keys.GetArrayLength().Should().Be(1);

        var key = keys[0];
        key.GetProperty("kty").GetString().Should().Be("RSA");
        key.GetProperty("use").GetString().Should().Be("sig");
        key.GetProperty("alg").GetString().Should().Be("RS256");
        key.GetProperty("kid").GetString().Should().Be("test-kid");
        key.GetProperty("n").GetString().Should().NotBeNullOrWhiteSpace();
        key.GetProperty("e").GetString().Should().NotBeNullOrWhiteSpace();

        // Ensure private key properties are not exposed
        key.TryGetProperty("d", out _).Should().BeFalse();
        key.TryGetProperty("p", out _).Should().BeFalse();
        key.TryGetProperty("q", out _).Should().BeFalse();
    }
}
