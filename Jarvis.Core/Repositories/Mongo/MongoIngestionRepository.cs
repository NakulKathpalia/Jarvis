using Jarvis.Ingestion;
using Jarvis.Mongo;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoIngestionRepository : IIngestionRepository
{
    private readonly IMongoCollection<IngestionJob> _jobs;

    public MongoIngestionRepository(MongoContext context)
    {
        _jobs = context.Collection<IngestionJob>(MongoCollectionNames.IngestionJobs);
    }

    public async Task<IReadOnlyList<IngestionJob>> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await _jobs.Find(job => job.UserId == userId)
            .SortByDescending(job => job.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IngestionJob?> GetAsync(string userId, string id, CancellationToken cancellationToken = default) =>
        await _jobs.Find(job => job.UserId == userId && job.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task UpsertAsync(string userId, IngestionJob job, CancellationToken cancellationToken = default)
    {
        job.UserId = userId;
        job.UpdatedAtUtc = DateTime.UtcNow;
        job.CreatedAtUtc = job.CreatedAtUtc == default ? job.UpdatedAtUtc : job.CreatedAtUtc;
        foreach (var candidate in job.Candidates)
        {
            candidate.UserId = userId;
            candidate.UpdatedAtUtc = candidate.UpdatedAtUtc == default ? DateTime.UtcNow : candidate.UpdatedAtUtc;
            candidate.CreatedAtUtc = candidate.CreatedAtUtc == default ? candidate.UpdatedAtUtc : candidate.CreatedAtUtc;
        }

        return _jobs.ReplaceOneAsync(
            stored => stored.UserId == userId && stored.Id == job.Id,
            job,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public Task DeleteAsync(string userId, string id, CancellationToken cancellationToken = default) =>
        _jobs.DeleteOneAsync(job => job.UserId == userId && job.Id == id, cancellationToken);
}
