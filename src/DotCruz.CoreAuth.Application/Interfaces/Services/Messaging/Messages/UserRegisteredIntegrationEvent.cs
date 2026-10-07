namespace DotCruz.CoreAuth.Application.Interfaces.Services.Messaging.Messages;

public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Name,
    string Email,
    DateTime OccurredOn
);
