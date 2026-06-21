using Jarvis.Models;

namespace Jarvis.Repositories;

public interface IVoiceHistoryRepository
{
    Task<IReadOnlyList<VoiceHistoryItem>> GetRecentAsync(string userId, int limit, CancellationToken cancellationToken = default);
    Task AddAsync(string userId, VoiceHistoryItem item, CancellationToken cancellationToken = default);
}
