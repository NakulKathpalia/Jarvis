using Jarvis.Models;

namespace Jarvis.Repositories;

public interface IChatRepository
{
    Task<IReadOnlyList<ChatSession>> GetSessionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<ChatSession?> GetSessionAsync(string userId, string id, CancellationToken cancellationToken = default);
    Task UpsertSessionAsync(string userId, ChatSession session, CancellationToken cancellationToken = default);
    Task AddMessageAsync(string userId, string sessionId, ChatMessage message, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(string userId, string id, CancellationToken cancellationToken = default);
}
