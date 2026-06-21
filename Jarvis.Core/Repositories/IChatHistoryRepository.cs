using Jarvis.Models;

namespace Jarvis.Repositories;

public interface IChatHistoryRepository
{
    Task<IReadOnlyList<ChatMessage>> GetAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(string userId, ChatMessage message, CancellationToken cancellationToken = default);
}
