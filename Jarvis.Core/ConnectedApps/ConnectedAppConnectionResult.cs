namespace Jarvis.ConnectedApps;

public sealed record ConnectedAppConnectionResult(
    bool Succeeded,
    string Message,
    ConnectedAppInfo? App);
