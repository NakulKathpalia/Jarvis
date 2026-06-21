using Jarvis.Users;

namespace Jarvis.Repositories;

public interface IDeviceRepository
{
    Task UpsertAsync(DeviceRecord device, CancellationToken cancellationToken = default);
    Task<DeviceRecord?> GetAsync(string userId, string deviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceRecord>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task RevokeAsync(string userId, string deviceId, CancellationToken cancellationToken = default);
}
