namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents raw repository information produced by repository readers.
/// </summary>
public sealed class RepositoryReadResult
{
    /// <summary>
    /// Gets or sets the repository root path.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets discovered folders.
    /// </summary>
    public List<RepositoryFolder> Folders { get; set; } = [];

    /// <summary>
    /// Gets or sets discovered files.
    /// </summary>
    public List<RepositoryFile> Files { get; set; } = [];

    /// <summary>
    /// Gets or sets discovered projects.
    /// </summary>
    public List<RepositoryProject> Projects { get; set; } = [];

    /// <summary>
    /// Gets or sets detected configurations.
    /// </summary>
    public List<RepositoryConfiguration> Configurations { get; set; } = [];
}
