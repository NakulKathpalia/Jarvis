namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents build statistics.
/// </summary>
public sealed class BuildStatistics
{
    /// <summary>
    /// Gets or sets build duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the exit code.
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    /// Gets or sets error count.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets warning count.
    /// </summary>
    public int WarningCount { get; set; }
}
