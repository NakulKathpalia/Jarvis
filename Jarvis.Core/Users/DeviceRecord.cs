namespace Jarvis.Users;

public sealed class DeviceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = Environment.MachineName;
    public string DeviceType { get; set; } = "Desktop";
    public string Platform { get; set; } = Environment.OSVersion.Platform.ToString();
    public bool Trusted { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
}
