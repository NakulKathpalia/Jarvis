namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a project reference dependency.
/// </summary>
public sealed class RepositoryDependency
{
    /// <summary>
    /// Gets or sets the source project name.
    /// </summary>
    public string SourceProject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the referenced project path.
    /// </summary>
    public string ReferencePath { get; set; } = string.Empty;
}
