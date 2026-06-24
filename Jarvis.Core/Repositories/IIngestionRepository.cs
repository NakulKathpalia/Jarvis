using Jarvis.Ingestion;

namespace Jarvis.Repositories;

public interface IIngestionRepository
{
    Task<IReadOnlyList<IngestionJob>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IngestionJob?> GetAsync(string userId, string id, CancellationToken cancellationToken = default);
    Task UpsertAsync(string userId, IngestionJob job, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userId, string id, CancellationToken cancellationToken = default);
}
