namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents repository search results.
/// </summary>
public sealed class SearchResult
{
    /// <summary>
    /// Gets or sets matched files.
    /// </summary>
    public List<RepositoryFile> Files { get; set; } = [];

    /// <summary>
    /// Gets or sets matched folders.
    /// </summary>
    public List<RepositoryFolder> Folders { get; set; } = [];

    /// <summary>
    /// Gets or sets matched projects.
    /// </summary>
    public List<RepositoryProject> Projects { get; set; } = [];

    /// <summary>
    /// Gets or sets matched configurations.
    /// </summary>
    public List<RepositoryConfiguration> Configurations { get; set; } = [];

    /// <summary>
    /// Gets or sets matched languages.
    /// </summary>
    public List<string> Languages { get; set; } = [];
}
