using Jarvis.Models;

namespace Jarvis.Repositories;

public interface IMemoryRepository
{
    Task<IReadOnlyList<MemoryItem>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task UpsertAsync(string userId, MemoryItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userId, string id, CancellationToken cancellationToken = default);
    Task ClearAsync(string userId, CancellationToken cancellationToken = default);
}
