namespace Jarvis.ConnectedApps;

public interface IConnectedAppService
{
    IReadOnlyCollection<ConnectedAppInfo> GetApps();
    ConnectedAppInfo? GetApp(string provider);
    ConnectedAppConnectionResult Connect(string provider);
    ConnectedAppConnectionResult Disconnect(string provider);
}
