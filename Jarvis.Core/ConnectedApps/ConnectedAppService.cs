namespace Jarvis.ConnectedApps;

public sealed class ConnectedAppService : IConnectedAppService
{
    private readonly Dictionary<string, ConnectedAppInfo> _apps;

    public ConnectedAppService(IEnumerable<ConnectedAppInfo>? apps = null)
    {
        _apps = (apps ?? GetDefaultApps()).ToDictionary(app => app.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<ConnectedAppInfo> GetApps() => _apps.Values.ToList();

    public ConnectedAppInfo? GetApp(string provider) => _apps.GetValueOrDefault(provider);

    public ConnectedAppConnectionResult Connect(string provider)
    {
        var app = GetApp(provider);
        // TODO: Real OAuth connection must require authentication, authorization, SecurityService review, and audit logging.
        return app is null
            ? new(false, "Unknown provider.", null)
            : new(false, "OAuth is not configured yet.", app);
    }

    public ConnectedAppConnectionResult Disconnect(string provider)
    {
        var app = GetApp(provider);
        // TODO: Real disconnect should revoke provider grants and write an audit record.
        return app is null
            ? new(false, "Unknown provider.", null)
            : new(true, $"{app.Name} was already disconnected.", app);
    }

    public static IReadOnlyCollection<ConnectedAppInfo> GetDefaultApps() =>
    [
        new(ConnectedAppProvider.Google, "google", "Google", ConnectedAppStatus.NeedsSetup, "Future Gmail, Calendar, and Drive connector.", false, ["Gmail", "Calendar", "Drive"]),
        new(ConnectedAppProvider.Microsoft, "microsoft", "Microsoft", ConnectedAppStatus.NeedsSetup, "Future Outlook, Calendar, and OneDrive connector.", false, ["Outlook", "Calendar", "OneDrive"]),
        new(ConnectedAppProvider.GitHub, "github", "GitHub", ConnectedAppStatus.NeedsSetup, "Future repository, issue, and pull request connector.", false, ["Repositories", "Issues", "Pull Requests"]),
        new(ConnectedAppProvider.Discord, "discord", "Discord", ConnectedAppStatus.NeedsSetup, "Future server, channel, and message connector.", false, ["Servers", "Channels", "Messages"])
    ];
}
