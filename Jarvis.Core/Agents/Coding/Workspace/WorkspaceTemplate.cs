namespace Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Describes a workspace template.
/// </summary>
public sealed class WorkspaceTemplate
{
    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = "project";

    /// <summary>
    /// Gets or sets the project folder name.
    /// </summary>
    public string ProjectFolderName { get; set; } = "project";
}
