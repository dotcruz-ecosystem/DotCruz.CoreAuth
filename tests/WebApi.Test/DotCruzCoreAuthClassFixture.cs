using System.Net.Http.Headers;
using System.Text.Json;

namespace WebApi.Test;

public class DotCruzCoreAuthClassFixture : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public DotCruzCoreAuthClassFixture(CustomWebApplicationFactory factory) => _httpClient = factory.CreateClient();

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    protected async Task<HttpResponseMessage> DoPost(
        string method,
        object request,
        string token = "",
        string culture = "en",
        string tenantId = "")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        AddTenantHeader(tenantId);

        return await _httpClient.PostAsJsonAsync(method, request, JsonSerializerOptions);
    }

    protected async Task<HttpResponseMessage> DoGet(
        string method,
        string token = "",
        string culture = "en",
        string tenantId = "")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        AddTenantHeader(tenantId);

        return await _httpClient.GetAsync(method);
    }

    protected async Task<HttpResponseMessage> DoPut(
        string method,
        object request,
        string token = "",
        string culture = "en",
        string tenantId = "")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        AddTenantHeader(tenantId);

        return await _httpClient.PutAsJsonAsync(method, request, JsonSerializerOptions);
    }

    protected async Task<HttpResponseMessage> DoPatch(
        string method,
        object request,
        string token = "",
        string culture = "en",
        string tenantId = "")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        AddTenantHeader(tenantId);

        return await _httpClient.PatchAsJsonAsync(method, request, JsonSerializerOptions);
    }

    protected async Task<HttpResponseMessage> DoDelete(
        string method,
        string token = "",
        string culture = "en",
        string tenantId = "")
    {
        ChangeRequestCulture(culture);
        AuthorizeRequest(token);
        AddTenantHeader(tenantId);

        return await _httpClient.DeleteAsync(method);
    }

    private void ChangeRequestCulture(string culture)
    {
        _httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
        if (!string.IsNullOrWhiteSpace(culture))
        {
            _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
        }
    }

    private void AuthorizeRequest(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private void AddTenantHeader(string tenantId)
    {
        _httpClient.DefaultRequestHeaders.Remove("X-Tenant-ID");
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Tenant-ID", tenantId);
        }
    }
}
