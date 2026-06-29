namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Collects passive in-memory workflow engine metrics.
/// </summary>
public sealed class WorkflowMetricsCollector
{
    private readonly object gate = new();
    private TimeSpan totalDuration;
    private int workflowCount;
    private int retries;
    private int parallelSteps;
    private int failures;
    private double lastCompletionRate;

    /// <summary>
    /// Records retries observed during step execution.
    /// </summary>
    /// <param name="count">The retry count.</param>
    public void RecordRetries(int count)
    {
        lock (gate)
        {
            retries += count;
        }
    }

    /// <summary>
    /// Records a parallel step batch.
    /// </summary>
    /// <param name="stepCount">The number of steps in the batch.</param>
    public void RecordParallelBatch(int stepCount)
    {
        if (stepCount <= 1)
        {
            return;
        }

        lock (gate)
        {
            parallelSteps += stepCount;
        }
    }

    /// <summary>
    /// Records a completed workflow result.
    /// </summary>
    /// <param name="result">The workflow result.</param>
    /// <param name="totalSteps">The total workflow step count.</param>
    public void RecordWorkflow(WorkflowResult result, int totalSteps)
    {
        ArgumentNullException.ThrowIfNull(result);

        lock (gate)
        {
            workflowCount++;
            failures += result.Errors.Count;
            totalDuration += result.Duration ?? TimeSpan.Zero;
            lastCompletionRate = totalSteps == 0 ? 0 : (double)result.StepResults.Count / totalSteps;
        }
    }

    /// <summary>
    /// Gets a read-only metrics snapshot.
    /// </summary>
    /// <returns>The metrics snapshot.</returns>
    public WorkflowMetrics GetSnapshot()
    {
        lock (gate)
        {
            return new WorkflowMetrics
            {
                ExecutionTime = totalDuration,
                Retries = retries,
                ParallelSteps = parallelSteps,
                Failures = failures,
                CompletionRate = Math.Round(lastCompletionRate, 4),
                AverageDuration = workflowCount == 0
                    ? TimeSpan.Zero
                    : TimeSpan.FromTicks(totalDuration.Ticks / workflowCount)
            };
        }
    }
}
