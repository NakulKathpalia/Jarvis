namespace Jarvis.Users;

public sealed class UserSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = Environment.MachineName;
    public SessionType SessionType { get; set; } = SessionType.Desktop;
    public bool TrustedDevice { get; set; } = true;
    public bool RememberMe { get; set; } = true;
    public string SessionHash { get; set; } = string.Empty;
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
