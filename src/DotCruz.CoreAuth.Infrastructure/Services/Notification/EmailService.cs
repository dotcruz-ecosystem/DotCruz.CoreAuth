using DotCruz.CoreAuth.Application.Interfaces.Services;
using DotCruz.Notifications.Contracts.Enums.Notifications;
using DotCruz.Notifications.Contracts.Messages.Notifications.CreateNotification;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net.Http.Json;

namespace DotCruz.CoreAuth.Infrastructure.Services.Notification;

public class EmailService(
    HttpClient httpClient,
    IConfiguration configuration
) : IEmailService
{
    private readonly Guid _serviceId = configuration.GetValue<Guid>("ServiceId");
    private readonly string _frontendUrl = configuration.GetValue<string>("Settings:FrontendUrl") ?? "http://localhost:3000";

    public async Task SendPasswordResetEmailAsync(string email, string name, string token, CancellationToken cancellationToken)
    {
        var message = new CreateNotificationMessage(
            ServiceId: _serviceId,
            Type: IntegrationNotificationType.Email,
            Recipient: email,
            Culture: CultureInfo.CurrentUICulture.Name,
            TemplateCode: "RequestPasswordResetCommand",
            TemplateData: new Dictionary<string, object> 
            { 
                { "name", name },
                { "link", $"{_frontendUrl}/reset-password?token={token}" }
            }
        );

        var response = await httpClient.PostAsJsonAsync("api/Notification", message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendWelcomeEmailAsync(string email, string name, CancellationToken cancellationToken)
    {
        var message = new CreateNotificationMessage(
            ServiceId: _serviceId,
            Type: IntegrationNotificationType.Email,
            Recipient: email,
            Culture: CultureInfo.CurrentUICulture.Name,
            TemplateCode: "CreateUserCommand",
            TemplateData: new Dictionary<string, object> 
            { 
                { "name", name }
            }
        );

        var response = await httpClient.PostAsJsonAsync("api/Notification", message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendActivationEmailAsync(string email, string name, string token, CancellationToken cancellationToken)
    {
        var message = new CreateNotificationMessage(
            ServiceId: _serviceId,
            Type: IntegrationNotificationType.Email,
            Recipient: email,
            Culture: CultureInfo.CurrentUICulture.Name,
            TemplateCode: "ActivateAccountCommand",
            TemplateData: new Dictionary<string, object> 
            { 
                { "name", name },
                { "link", $"{_frontendUrl}/activate?token={token}" }
            }
        );

        var response = await httpClient.PostAsJsonAsync("api/Notification", message, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
