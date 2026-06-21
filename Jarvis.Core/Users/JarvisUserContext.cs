namespace Jarvis.Users;

public sealed class JarvisUserContext
{
    public const string DefaultOwnerUserId = "local-owner";

    public string UserId { get; init; } = DefaultOwnerUserId;
    public string DeviceId { get; init; } = Environment.MachineName;
}
