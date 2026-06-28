namespace Jarvis.Core.Core.Logging;

/// <summary>
/// Stores recent framework log entries in memory.
/// </summary>
public sealed class InMemoryFrameworkLogger : IFrameworkLogger
{
    private readonly Queue<FrameworkLogEntry> entries = [];
    private readonly object gate = new();
    private readonly int capacity;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryFrameworkLogger"/> class.
    /// </summary>
    /// <param name="capacity">The maximum number of entries to retain.</param>
    public InMemoryFrameworkLogger(int capacity = 250)
    {
        this.capacity = Math.Max(1, capacity);
    }

    /// <summary>
    /// Gets the retained log entries.
    /// </summary>
    public IReadOnlyCollection<FrameworkLogEntry> Entries
    {
        get
        {
            lock (gate)
            {
                return entries.ToArray();
            }
        }
    }

    /// <inheritdoc />
    public Task LogAsync(FrameworkLogEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (gate)
        {
            entries.Enqueue(entry);
            while (entries.Count > capacity)
            {
                entries.Dequeue();
            }
        }

        return Task.CompletedTask;
    }
}
