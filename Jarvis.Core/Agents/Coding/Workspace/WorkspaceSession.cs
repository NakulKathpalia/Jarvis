namespace Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Represents one managed runnable workspace session.
/// </summary>
public sealed class WorkspaceSession
{
    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    public string Id { get; set; } = "run-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

    /// <summary>
    /// Gets or sets the workspace root path.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project path.
    /// </summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this writes into the repository.
    /// </summary>
    public bool ApplyToRepository { get; set; }

    /// <summary>
    /// Gets workspace files.
    /// </summary>
    public List<WorkspaceFile> Files { get; } = [];
}
