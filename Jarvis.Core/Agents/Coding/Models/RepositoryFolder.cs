namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a folder discovered in a repository.
/// </summary>
public sealed class RepositoryFolder
{
    /// <summary>
    /// Gets or sets the absolute folder path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository-relative folder path.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the folder name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
