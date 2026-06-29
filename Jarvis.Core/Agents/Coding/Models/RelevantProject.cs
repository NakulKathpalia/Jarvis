namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a project selected as factual coding context.
/// </summary>
public sealed class RelevantProject
{
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project type.
    /// </summary>
    public string ProjectType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets project reference paths.
    /// </summary>
    public List<string> ProjectReferences { get; set; } = [];
}
