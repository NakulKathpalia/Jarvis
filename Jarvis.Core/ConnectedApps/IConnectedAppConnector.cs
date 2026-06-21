namespace Jarvis.ConnectedApps;

public interface IConnectedAppConnector
{
    ConnectedAppProvider Provider { get; }
    Task<bool> CanExecuteAsync(string action, CancellationToken cancellationToken = default);
}
