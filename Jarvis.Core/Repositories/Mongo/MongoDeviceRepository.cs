using Jarvis.Mongo;
using Jarvis.Users;
using MongoDB.Driver;

namespace Jarvis.Repositories.Mongo;

public sealed class MongoDeviceRepository : IDeviceRepository
{
    private readonly IMongoCollection<DeviceRecord> _devices;

    public MongoDeviceRepository(MongoContext context)
    {
        _devices = context.Collection<DeviceRecord>(MongoCollectionNames.Devices);
    }

    public Task UpsertAsync(DeviceRecord device, CancellationToken cancellationToken = default)
    {
        device.UpdatedAtUtc = DateTime.UtcNow;
        return _devices.ReplaceOneAsync(
            existing => existing.UserId == device.UserId && existing.Id == device.Id,
            device,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }

    public async Task<DeviceRecord?> GetAsync(string userId, string deviceId, CancellationToken cancellationToken = default) =>
        await _devices.Find(device => device.UserId == userId && device.Id == deviceId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<DeviceRecord>> GetForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await _devices.Find(device => device.UserId == userId && device.RevokedAtUtc == null)
            .SortByDescending(device => device.LastSeenAtUtc)
            .ToListAsync(cancellationToken);

    public Task RevokeAsync(string userId, string deviceId, CancellationToken cancellationToken = default) =>
        _devices.UpdateOneAsync(
            device => device.UserId == userId && device.Id == deviceId,
            Builders<DeviceRecord>.Update
                .Set(device => device.RevokedAtUtc, DateTime.UtcNow)
                .Set(device => device.UpdatedAtUtc, DateTime.UtcNow),
            cancellationToken: cancellationToken);
}
