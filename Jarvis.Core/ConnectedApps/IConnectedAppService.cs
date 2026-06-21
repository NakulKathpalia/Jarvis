namespace Jarvis.ConnectedApps;

public interface IConnectedAppService
{
    IReadOnlyCollection<ConnectedAppInfo> GetApps();
    ConnectedAppInfo? GetApp(string provider);
    ConnectedAppOperationResult Connect(string provider);
    ConnectedAppOperationResult Disconnect(string provider);
}

public sealed record ConnectedAppOperationResult(bool Succeeded, string Message, ConnectedAppInfo? App);
