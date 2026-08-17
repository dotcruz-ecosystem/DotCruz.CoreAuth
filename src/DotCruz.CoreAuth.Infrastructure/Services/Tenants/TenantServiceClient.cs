using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants;
using DotCruz.CoreAuth.Application.Interfaces.Services.Tenants.Responses;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DotCruz.CoreAuth.Infrastructure.Services.Tenants;

public class TenantServiceClient : ITenantServiceClient
{
    private const string TENANT_SUMMARY_ENDPOINT = "api/tenants/{0}/summary";

    private readonly HttpClient _httpClient;
    private readonly ILogger<TenantServiceClient> _logger;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public TenantServiceClient(HttpClient httpClient, ILogger<TenantServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TenantSummaryDto?> GetTenantSummaryAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(string.Format(TENANT_SUMMARY_ENDPOINT, tenantId), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch tenant summary for TenantId {TenantId}. StatusCode: {StatusCode}", tenantId, response.StatusCode);
            await HandleErrorResponse(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<TenantSummaryDto>(_serializerOptions, cancellationToken: cancellationToken);
    }

    private async Task HandleErrorResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errors = await GetErrorsAsync(response, cancellationToken);
        var primaryError = errors.FirstOrDefault() ?? ResourceMessagesException.UNKNOWN_ERROR_TENANT;

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest => new ErrorOnValidationException(errors),
            HttpStatusCode.NotFound => new NotFoundException(primaryError),
            HttpStatusCode.Conflict => new ConflictException(primaryError),
            HttpStatusCode.Unauthorized => new UnauthorizedException(primaryError),
            HttpStatusCode.Forbidden => new UnauthorizedException(primaryError),
            _ => new InfrastructureException(string.Format(ResourceMessagesException.EXTERNAL_INTEGRATION_ERROR, response.StatusCode, primaryError))
        };
    }

    private async Task<IEnumerable<string>> GetErrorsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<TenantErrorResponse>(_serializerOptions, cancellationToken);
            return errorResponse?.Errors ?? [];
        }
        catch
        {
            return [];
        }
    }
}
