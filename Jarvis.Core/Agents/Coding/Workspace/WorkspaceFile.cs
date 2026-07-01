namespace Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Represents a file created in a managed workspace.
/// </summary>
public sealed class WorkspaceFile
{
    /// <summary>
    /// Gets or sets the relative file path.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute file path.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;
}
