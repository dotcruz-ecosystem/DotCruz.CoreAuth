using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging;
using DotCruz.CoreAuth.Application.Interfaces.Services.Messaging.Messages;
using Moq;

namespace CommonTestUtilities.Services;

public class WorkspaceProvisioningPublisherBuilder
{
    public static IWorkspaceProvisioningPublisher Build()
    {
        var mock = new Mock<IWorkspaceProvisioningPublisher>();
        mock.Setup(x => x.PublishUserRegisteredAsync(
            It.IsAny<UserRegisteredIntegrationEvent>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mock.Object;
    }
}
