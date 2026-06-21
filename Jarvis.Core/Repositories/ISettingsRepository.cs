using Jarvis.Models;

namespace Jarvis.Repositories;

public interface ISettingsRepository
{
    Task<AppSettings?> GetAsync(string userId, string scope, CancellationToken cancellationToken = default);
    Task UpsertAsync(string userId, string scope, AppSettings settings, CancellationToken cancellationToken = default);
}
