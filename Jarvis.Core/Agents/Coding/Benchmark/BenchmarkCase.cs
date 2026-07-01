namespace Jarvis.Core.Agents.Coding.Benchmark;

/// <summary>
/// Represents one benchmark prompt.
/// </summary>
public sealed class BenchmarkCase
{
    /// <summary>
    /// Gets or sets benchmark name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets prompt.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets category.
    /// </summary>
    public BenchmarkCategory Category { get; set; } = BenchmarkCategory.RunnableUi;

    /// <summary>
    /// Gets expected generated file names.
    /// </summary>
    public List<string> ExpectedFiles { get; } = [];
}
