namespace Jarvis.ConnectedApps;

public sealed class ConnectedAppInfo
{
    public ConnectedAppInfo()
    {
    }

    public ConnectedAppInfo(
        ConnectedAppProvider provider,
        string id,
        string name,
        ConnectedAppStatus status,
        string description,
        bool configured,
        IReadOnlyList<string> capabilities)
    {
        Provider = provider;
        Id = id;
        Name = name;
        Status = status;
        Description = description;
        Configured = configured;
        Capabilities = capabilities.ToList();
    }

    public string UserId { get; set; } = string.Empty;
    public ConnectedAppProvider Provider { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ConnectedAppStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Configured { get; set; }
    public IReadOnlyList<string> Capabilities { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
