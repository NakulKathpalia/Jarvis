namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Represents current execution progress.
/// </summary>
public sealed class ExecutionProgress
{
    /// <summary>
    /// Gets or sets current stage.
    /// </summary>
    public ExecutionStage CurrentStage { get; set; } = ExecutionStage.RequestReceived;

    /// <summary>
    /// Gets or sets elapsed time.
    /// </summary>
    public TimeSpan Elapsed { get; set; }

    /// <summary>
    /// Gets or sets estimated remaining time.
    /// </summary>
    public TimeSpan EstimatedRemaining { get; set; }

    /// <summary>
    /// Gets or sets processed file count.
    /// </summary>
    public int FilesProcessed { get; set; }

    /// <summary>
    /// Gets or sets processed symbol count.
    /// </summary>
    public int SymbolsProcessed { get; set; }

    /// <summary>
    /// Gets or sets current model.
    /// </summary>
    public string CurrentModel { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets current provider.
    /// </summary>
    public string CurrentProvider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets current tool.
    /// </summary>
    public string CurrentTool { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets current agent.
    /// </summary>
    public string CurrentAgent { get; set; } = string.Empty;
}
