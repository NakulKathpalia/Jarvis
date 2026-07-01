namespace Jarvis.Core.Agents.Coding.Execution;

using System.Diagnostics;

/// <summary>
/// Reports live coding execution events.
/// </summary>
public sealed class ExecutionReporter
{
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private readonly Dictionary<ExecutionStage, TimeSpan> stageStarts = [];

    /// <summary>
    /// Gets execution timeline.
    /// </summary>
    public ExecutionTimeline Timeline { get; } = new();

    /// <summary>
    /// Gets execution metrics.
    /// </summary>
    public ExecutionMetrics Metrics { get; } = new();

    /// <summary>
    /// Occurs when an event is emitted.
    /// </summary>
    public event EventHandler<ExecutionEvent>? EventEmitted;

    /// <summary>
    /// Starts a stage.
    /// </summary>
    public void Start(ExecutionStage stage, string message, string agent = "", string tool = "")
    {
        stageStarts[stage] = stopwatch.Elapsed;
        Emit(stage, ExecutionEventType.Started, ExecutionStatus.Active, message, agent, tool);
    }

    /// <summary>
    /// Completes a stage.
    /// </summary>
    public void Complete(ExecutionStage stage, string message, string agent = "", string tool = "")
    {
        if (stageStarts.TryGetValue(stage, out var started))
        {
            Metrics.Record(stage, stopwatch.Elapsed - started);
        }

        Emit(stage, ExecutionEventType.Completed, ExecutionStatus.Succeeded, message, agent, tool);
    }

    /// <summary>
    /// Emits a warning.
    /// </summary>
    public void Warn(ExecutionStage stage, string message)
    {
        Emit(stage, ExecutionEventType.Warning, ExecutionStatus.Active, message);
    }

    /// <summary>
    /// Emits an error.
    /// </summary>
    public void Error(ExecutionStage stage, string message)
    {
        Emit(stage, ExecutionEventType.Error, ExecutionStatus.Failed, message);
    }

    /// <summary>
    /// Updates model and provider progress.
    /// </summary>
    public void SetModel(string provider, string model)
    {
        Timeline.Session.Progress.CurrentProvider = provider;
        Timeline.Session.Progress.CurrentModel = model;
    }

    /// <summary>
    /// Updates processed file and symbol counts.
    /// </summary>
    public void SetProcessedCounts(int files, int symbols)
    {
        Timeline.Session.Progress.FilesProcessed = files;
        Timeline.Session.Progress.SymbolsProcessed = symbols;
    }

    private void Emit(
        ExecutionStage stage,
        ExecutionEventType type,
        ExecutionStatus status,
        string message,
        string agent = "",
        string tool = "")
    {
        Timeline.Session.State = type == ExecutionEventType.Completed && stage == ExecutionStage.Finished
            ? ExecutionState.Completed
            : ExecutionState.Running;
        var executionEvent = new ExecutionEvent
        {
            Stage = stage,
            Type = type,
            Status = status,
            Message = message,
            Agent = agent,
            Tool = tool,
            Elapsed = stopwatch.Elapsed
        };
        Timeline.Add(executionEvent);
        EventEmitted?.Invoke(this, executionEvent);
    }
}
