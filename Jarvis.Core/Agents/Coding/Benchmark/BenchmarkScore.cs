namespace Jarvis.Core.Agents.Coding.Benchmark;

/// <summary>
/// Represents benchmark scoring dimensions.
/// </summary>
public sealed class BenchmarkScore
{
    /// <summary>
    /// Gets or sets correctness score.
    /// </summary>
    public int Correctness { get; set; }

    /// <summary>
    /// Gets or sets simplicity score.
    /// </summary>
    public int Simplicity { get; set; }

    /// <summary>
    /// Gets or sets architecture score.
    /// </summary>
    public int Architecture { get; set; }

    /// <summary>
    /// Gets or sets readability score.
    /// </summary>
    public int Readability { get; set; }

    /// <summary>
    /// Gets or sets runtime score.
    /// </summary>
    public int Runtime { get; set; }

    /// <summary>
    /// Gets average score.
    /// </summary>
    public double Average => (Correctness + Simplicity + Architecture + Readability + Runtime) / 5.0;
}
