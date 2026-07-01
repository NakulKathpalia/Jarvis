namespace Jarvis.Core.Agents.Coding.Runtime;

/// <summary>
/// Represents a locally running generated project.
/// </summary>
public sealed class RunningProject
{
    /// <summary>
    /// Gets or sets a value indicating whether the project is running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets the selected port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the local URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the process identifier.
    /// </summary>
    public int? ProcessId { get; set; }

    /// <summary>
    /// Gets or sets server status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets stop command.
    /// </summary>
    public string StopCommand { get; set; } = string.Empty;

    /// <summary>
    /// Gets logs or error details.
    /// </summary>
    public List<string> Logs { get; } = [];
}
