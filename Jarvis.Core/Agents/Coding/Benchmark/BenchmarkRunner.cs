namespace Jarvis.Core.Agents.Coding.Benchmark;

using System.Diagnostics;
using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Runs benchmark cases.
/// </summary>
public sealed class BenchmarkRunner
{
    private readonly RunnableExecutor runnableExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkRunner"/> class.
    /// </summary>
    public BenchmarkRunner(RunnableExecutor? runnableExecutor = null)
    {
        this.runnableExecutor = runnableExecutor ?? new RunnableExecutor();
    }

    /// <summary>
    /// Runs one benchmark case.
    /// </summary>
    public BenchmarkResult Run(BenchmarkCase benchmarkCase, string repositoryPath)
    {
        var stopwatch = Stopwatch.StartNew();
        var runnable = runnableExecutor.Execute(new RunnableRequest
        {
            RepositoryPath = repositoryPath,
            Prompt = benchmarkCase.Prompt
        });
        stopwatch.Stop();
        var generated = runnable.CreatedFiles.Select(file => file.RelativePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var expectedFound = benchmarkCase.ExpectedFiles.Count == 0 ||
            benchmarkCase.ExpectedFiles.All(generated.Contains);
        return new BenchmarkResult
        {
            Case = benchmarkCase,
            RunnableResult = runnable,
            Duration = stopwatch.Elapsed,
            Passed = runnable.Succeeded && expectedFound,
            Score = Score(runnable, expectedFound)
        };
    }

    private static BenchmarkScore Score(RunnableResult runnable, bool expectedFound)
    {
        return new BenchmarkScore
        {
            Correctness = runnable.Succeeded && expectedFound ? 100 : 40,
            Simplicity = runnable.CreatedFiles.Count <= 5 ? 100 : 80,
            Architecture = runnable.WorkspacePath.Contains(".jarvis-runs", StringComparison.OrdinalIgnoreCase) ? 100 : 60,
            Readability = runnable.CreatedFiles.Any(file => file.RelativePath.EndsWith(".css", StringComparison.OrdinalIgnoreCase)) ? 95 : 75,
            Runtime = runnable.Succeeded ? 100 : 30
        };
    }
}
