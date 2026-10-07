namespace DotCruz.CoreAuth.Common.Settings;

public class WorkspaceMessagingSettings
{
    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string Username { get; set; } = "dotcruz";
    public string Password { get; set; } = "mudar123";
    public bool UseInMemory { get; set; } = false;
}
