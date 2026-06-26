using Jarvis.Models;

namespace Jarvis.Repositories;

public interface IKnowledgeRepository
{
    Task<IReadOnlyList<KnowledgeItem>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task UpsertAsync(string userId, KnowledgeItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userId, string id, CancellationToken cancellationToken = default);
}
