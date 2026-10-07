using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging;
using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DotCruz.CoreAuth.Infrastructure.Services.Messaging;

public class WorkspaceProvisioningPublisher : IWorkspaceProvisioningPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<WorkspaceProvisioningPublisher> _logger;

    public WorkspaceProvisioningPublisher(
        IPublishEndpoint publishEndpoint,
        ILogger<WorkspaceProvisioningPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublishUserRegisteredAsync(UserRegisteredIntegrationEvent message, CancellationToken cancellationToken)
    {
        try
        {
            await _publishEndpoint.Publish(message, cancellationToken);
            _logger.LogInformation("Published UserRegisteredIntegrationEvent to RabbitMQ for UserId: {UserId}", message.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UserRegisteredIntegrationEvent to RabbitMQ for UserId: {UserId}", message.UserId);
            throw;
        }
    }
}
