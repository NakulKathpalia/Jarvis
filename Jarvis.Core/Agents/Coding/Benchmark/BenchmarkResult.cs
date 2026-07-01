namespace Jarvis.Core.Agents.Coding.Benchmark;

using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Represents a benchmark result.
/// </summary>
public sealed class BenchmarkResult
{
    /// <summary>
    /// Gets or sets benchmark case.
    /// </summary>
    public BenchmarkCase Case { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the case passed.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Gets or sets benchmark score.
    /// </summary>
    public BenchmarkScore Score { get; set; } = new();

    /// <summary>
    /// Gets or sets runnable result.
    /// </summary>
    public RunnableResult RunnableResult { get; set; } = new();

    /// <summary>
    /// Gets or sets duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
