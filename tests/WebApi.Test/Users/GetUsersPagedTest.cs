using FluentAssertions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace WebApi.Test.Users;

public class GetUsersPagedTest : DotCruzCoreAuthClassFixture
{
    private readonly string METHOD = "api/users";

    private readonly Guid _tenantId;

    public GetUsersPagedTest(CustomWebApplicationFactory factory) : base(factory)
    {
        _tenantId = factory.GetTenantId().GetValueOrDefault();
    }

    [Fact]
    public async Task Success()
    {
        var response = await DoGet(method: $"{METHOD}?page_number=1&page_size=10", tenantId: _tenantId.ToString());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var responseBody = await response.Content.ReadAsStreamAsync();
        var responseData = await JsonDocument.ParseAsync(responseBody);

        responseData.RootElement.GetProperty("page_number").GetInt32().Should().Be(1);
        responseData.RootElement.GetProperty("page_size").GetInt32().Should().Be(10);
        responseData.RootElement.GetProperty("total_count").GetInt32().Should().BeGreaterThanOrEqualTo(2);
        responseData.RootElement.GetProperty("total_pages").GetInt32().Should().BeGreaterThanOrEqualTo(1);

        var items = responseData.RootElement.GetProperty("items").EnumerateArray();
        items.Should().NotBeEmpty();
    }
}
