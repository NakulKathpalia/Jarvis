namespace Jarvis.Core.Agents.Coding.Benchmark;

using System.Text;

/// <summary>
/// Executes benchmark suites and stores benchmark history.
/// </summary>
public sealed class BenchmarkEngine
{
    private readonly BenchmarkRunner runner;
    private readonly List<BenchmarkResult> history = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkEngine"/> class.
    /// </summary>
    public BenchmarkEngine(BenchmarkRunner? runner = null)
    {
        this.runner = runner ?? new BenchmarkRunner();
    }

    /// <summary>
    /// Runs a benchmark suite.
    /// </summary>
    public IReadOnlyList<BenchmarkResult> Run(BenchmarkSuite suite, string repositoryPath)
    {
        var results = suite.Cases.Select(benchmarkCase => runner.Run(benchmarkCase, repositoryPath)).ToList();
        history.AddRange(results);
        return results;
    }

    /// <summary>
    /// Gets benchmark history.
    /// </summary>
    public IReadOnlyList<BenchmarkResult> History()
    {
        return history.ToList();
    }

    /// <summary>
    /// Exports a benchmark report.
    /// </summary>
    public string ExportReport(IEnumerable<BenchmarkResult> results)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Jarvis Coder Benchmark Report");
        foreach (var result in results)
        {
            builder.AppendLine($"- {result.Case.Name}: passed={result.Passed}, score={result.Score.Average:n1}, url={result.RunnableResult.Url}");
        }

        return builder.ToString();
    }
}
