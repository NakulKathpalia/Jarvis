namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Represents a safe workspace for a runnable task.
/// </summary>
public sealed class RunnableWorkspace
{
    /// <summary>
    /// Gets or sets the workspace root.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the run identifier.
    /// </summary>
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the workspace exists.
    /// </summary>
    public bool Exists => Directory.Exists(RootPath);
}
