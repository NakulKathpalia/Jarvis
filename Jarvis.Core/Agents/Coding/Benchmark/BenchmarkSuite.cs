namespace Jarvis.Core.Agents.Coding.Benchmark;

/// <summary>
/// Represents a suite of benchmark cases.
/// </summary>
public sealed class BenchmarkSuite
{
    /// <summary>
    /// Gets or sets suite name.
    /// </summary>
    public string Name { get; set; } = "Jarvis Coder Benchmarks";

    /// <summary>
    /// Gets benchmark cases.
    /// </summary>
    public List<BenchmarkCase> Cases { get; } = [];
}
