using Jarvis.Migrations;

namespace Jarvis.Repositories;

public interface IMigrationRepository
{
    Task<bool> HasCompletedAsync(string name, string version, CancellationToken cancellationToken = default);
    Task CompleteAsync(string name, string version, string message, CancellationToken cancellationToken = default);
}
