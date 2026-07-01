namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Tracks aggregate execution timing metrics.
/// </summary>
public sealed class ExecutionMetrics
{
    private readonly Dictionary<ExecutionStage, List<TimeSpan>> durations = [];

    /// <summary>
    /// Records a stage duration.
    /// </summary>
    public void Record(ExecutionStage stage, TimeSpan duration)
    {
        if (!durations.TryGetValue(stage, out var values))
        {
            values = [];
            durations[stage] = values;
        }

        values.Add(duration);
    }

    /// <summary>
    /// Gets average duration for a stage.
    /// </summary>
    public TimeSpan Average(ExecutionStage stage)
    {
        return durations.TryGetValue(stage, out var values) && values.Count > 0
            ? TimeSpan.FromTicks((long)values.Average(value => value.Ticks))
            : TimeSpan.Zero;
    }

    /// <summary>
    /// Gets average repository scan time.
    /// </summary>
    public TimeSpan AverageRepositoryScanTime => Average(ExecutionStage.RepositoryScan);

    /// <summary>
    /// Gets average planning time.
    /// </summary>
    public TimeSpan AveragePlanningTime => Average(ExecutionStage.Planning);

    /// <summary>
    /// Gets average AI time.
    /// </summary>
    public TimeSpan AverageAITime => Average(ExecutionStage.AIRequest);

    /// <summary>
    /// Gets average review time.
    /// </summary>
    public TimeSpan AverageReviewTime => Average(ExecutionStage.Review);

    /// <summary>
    /// Gets average patch time.
    /// </summary>
    public TimeSpan AveragePatchTime => Average(ExecutionStage.PatchPreview);

    /// <summary>
    /// Gets average build time.
    /// </summary>
    public TimeSpan AverageBuildTime => Average(ExecutionStage.Build);
}
