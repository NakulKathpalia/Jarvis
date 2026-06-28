namespace Jarvis.Core.Core.Logging;

/// <summary>
/// Provides a no-op framework logger.
/// </summary>
public sealed class NullFrameworkLogger : IFrameworkLogger
{
    /// <inheritdoc />
    public Task LogAsync(FrameworkLogEntry entry, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
