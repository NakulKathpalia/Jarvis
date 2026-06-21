namespace Jarvis.ConnectedApps;

public sealed class ConnectedAppService : IConnectedAppService
{
    private readonly Dictionary<string, ConnectedAppInfo> _apps = new(StringComparer.OrdinalIgnoreCase)
    {
        ["google"] = new(ConnectedAppProvider.Google, "google", "Google", ConnectedAppStatus.NeedsSetup, "Future Gmail, Drive, and Calendar connector."),
        ["microsoft"] = new(ConnectedAppProvider.Microsoft, "microsoft", "Microsoft", ConnectedAppStatus.NeedsSetup, "Future Outlook, OneDrive, and Teams connector."),
        ["github"] = new(ConnectedAppProvider.GitHub, "github", "GitHub", ConnectedAppStatus.NeedsSetup, "Future repository, issue, and pull request connector."),
        ["discord"] = new(ConnectedAppProvider.Discord, "discord", "Discord", ConnectedAppStatus.NeedsSetup, "Future server and message connector.")
    };

    public IReadOnlyCollection<ConnectedAppInfo> GetApps() => _apps.Values.ToList();

    public ConnectedAppInfo? GetApp(string provider) => _apps.GetValueOrDefault(provider);

    public ConnectedAppOperationResult Connect(string provider)
    {
        var app = GetApp(provider);
        return app is null
            ? new(false, "Unknown provider.", null)
            : new(false, $"{app.Name} requires OAuth setup before connecting.", app);
    }

    public ConnectedAppOperationResult Disconnect(string provider)
    {
        var app = GetApp(provider);
        return app is null
            ? new(false, "Unknown provider.", null)
            : new(true, $"{app.Name} was already disconnected.", app);
    }
}
