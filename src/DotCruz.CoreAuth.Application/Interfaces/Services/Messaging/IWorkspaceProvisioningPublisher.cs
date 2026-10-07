using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging.Messages;

namespace DotCruz.CoreAuth.Application.Interfaces.Services.Messaging;

public interface IWorkspaceProvisioningPublisher
{
    Task PublishUserRegisteredAsync(UserRegisteredIntegrationEvent message, CancellationToken cancellationToken);
}
