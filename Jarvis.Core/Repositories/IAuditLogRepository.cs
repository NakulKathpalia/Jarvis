using Jarvis.Models;

namespace Jarvis.Repositories;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<InteractionLogEntry>> GetRecentAsync(string userId, int limit, CancellationToken cancellationToken = default);
    Task AddAsync(string userId, InteractionLogEntry entry, CancellationToken cancellationToken = default);
    Task ClearAsync(string userId, CancellationToken cancellationToken = default);
}
