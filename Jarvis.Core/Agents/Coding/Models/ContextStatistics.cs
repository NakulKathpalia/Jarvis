namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents factual statistics for a context package.
/// </summary>
public sealed class ContextStatistics
{
    /// <summary>
    /// Gets or sets the relevant project count.
    /// </summary>
    public int RelevantProjectCount { get; set; }

    /// <summary>
    /// Gets or sets the relevant file count.
    /// </summary>
    public int RelevantFileCount { get; set; }

    /// <summary>
    /// Gets or sets the relevant symbol count.
    /// </summary>
    public int RelevantSymbolCount { get; set; }

    /// <summary>
    /// Gets or sets the dependency count.
    /// </summary>
    public int DependencyCount { get; set; }
}
