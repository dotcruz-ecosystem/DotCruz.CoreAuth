using System.ComponentModel.DataAnnotations;

namespace DotCruz.CoreAuth.Common.Settings;

public class NotificationSettings
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
}
