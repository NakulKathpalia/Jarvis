namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a project file discovered in a repository.
/// </summary>
public sealed class RepositoryProject
{
    /// <summary>
    /// Gets or sets the absolute project path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository-relative project path.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected project type.
    /// </summary>
    public string ProjectType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets project references discovered in the project file.
    /// </summary>
    public List<RepositoryDependency> ProjectReferences { get; set; } = [];
}
