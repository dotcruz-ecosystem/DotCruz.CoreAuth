using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces;
using DotCruz.CoreAuth.Application.Interfaces.Services.Workspaces.Responses;
using DotCruz.CoreAuth.Domain.Exceptions.BaseExceptions;
using DotCruz.CoreAuth.Domain.Exceptions.Resources;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DotCruz.CoreAuth.Infrastructure.Services.Workspaces;

public class WorkspaceServiceClient : IWorkspaceServiceClient
{
    private const string WORKSPACE_SUMMARY_ENDPOINT = "api/workspaces/{0}/summary";
    private const string USER_WORKSPACES_ENDPOINT = "api/workspaces/users/{0}/workspaces";

    private readonly HttpClient _httpClient;
    private readonly ILogger<WorkspaceServiceClient> _logger;

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public WorkspaceServiceClient(HttpClient httpClient, ILogger<WorkspaceServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<WorkspaceSummaryDto?> GetWorkspaceSummaryAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(string.Format(WORKSPACE_SUMMARY_ENDPOINT, workspaceId), cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to fetch workspace summary for WorkspaceId {WorkspaceId}. StatusCode: {StatusCode}", workspaceId, response.StatusCode);
            await HandleErrorResponse(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<WorkspaceSummaryDto>(_serializerOptions, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserWorkspaceDto>> GetUserWorkspacesAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(string.Format(USER_WORKSPACES_ENDPOINT, userId), cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch user workspaces for UserId {UserId}. StatusCode: {StatusCode}", userId, response.StatusCode);
                return [];
            }

            var workspaces = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<UserWorkspaceDto>>(_serializerOptions, cancellationToken: cancellationToken);
            return workspaces ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user workspaces for UserId {UserId}", userId);
            return [];
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errors = await GetErrorsAsync(response, cancellationToken);
        var primaryError = errors.FirstOrDefault() ?? ResourceMessagesException.UNKNOWN_ERROR_WORKSPACE;

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
            var errorResponse = await response.Content.ReadFromJsonAsync<WorkspaceErrorResponse>(_serializerOptions, cancellationToken);
            return errorResponse?.Errors ?? [];
        }
        catch
        {
            return [];
        }
    }
}
