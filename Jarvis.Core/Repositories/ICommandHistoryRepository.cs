using Jarvis.Models;

namespace Jarvis.Repositories;

public interface ICommandHistoryRepository
{
    Task<IReadOnlyList<PcCommandLogEntry>> GetRecentAsync(string userId, int limit, CancellationToken cancellationToken = default);
    Task AddAsync(string userId, PcCommandLogEntry entry, CancellationToken cancellationToken = default);
}
