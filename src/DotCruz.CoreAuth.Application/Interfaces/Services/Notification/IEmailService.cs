namespace DotCruz.CoreAuth.Application.Interfaces.Services.Notification;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string name, string token, CancellationToken cancellationToken);
    Task SendWelcomeEmailAsync(string email, string name, CancellationToken cancellationToken);
    Task SendActivationEmailAsync(string email, string name, string token, CancellationToken cancellationToken);
}
