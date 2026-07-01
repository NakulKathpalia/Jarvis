namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Represents the result of a runnable task.
/// </summary>
public sealed class RunnableResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the task succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the task type.
    /// </summary>
    public RunnableTaskType TaskType { get; set; }

    /// <summary>
    /// Gets or sets the workspace path.
    /// </summary>
    public string WorkspacePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets created files.
    /// </summary>
    public List<RunnableFile> CreatedFiles { get; } = [];

    /// <summary>
    /// Gets or sets selected port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the localhost URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets server status.
    /// </summary>
    public string ServerStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets server process identifier.
    /// </summary>
    public int? ProcessId { get; set; }

    /// <summary>
    /// Gets or sets stop instructions.
    /// </summary>
    public string StopInstructions { get; set; } = string.Empty;

    /// <summary>
    /// Gets errors.
    /// </summary>
    public List<string> Errors { get; } = [];

    /// <summary>
    /// Gets runtime logs.
    /// </summary>
    public List<string> Logs { get; } = [];

    /// <summary>
    /// Gets or sets execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}
