namespace Jarvis.Core.Core.Logging;

/// <summary>
/// Defines a lightweight logging hook for the framework runtime.
/// </summary>
public interface IFrameworkLogger
{
    /// <summary>
    /// Writes a framework log entry.
    /// </summary>
    /// <param name="entry">The log entry to write.</param>
    /// <param name="cancellationToken">A token that cancels logging.</param>
    /// <returns>A task that completes when logging has finished.</returns>
    Task LogAsync(FrameworkLogEntry entry, CancellationToken cancellationToken = default);
}
