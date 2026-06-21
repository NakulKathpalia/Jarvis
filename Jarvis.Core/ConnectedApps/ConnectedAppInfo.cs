namespace Jarvis.ConnectedApps;

public sealed record ConnectedAppInfo(
    ConnectedAppProvider Provider,
    string Id,
    string Name,
    ConnectedAppStatus Status,
    string Description,
    bool Configured,
    IReadOnlyList<string> Capabilities);
