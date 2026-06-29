namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a factual repository index.
/// </summary>
public sealed class RepositoryIndex
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
    /// Gets or sets the file index.
    /// </summary>
    public FileIndex Files { get; set; } = new();

    /// <summary>
    /// Gets or sets the project index.
    /// </summary>
    public ProjectIndex Projects { get; set; } = new();

    /// <summary>
    /// Gets or sets repository folders.
    /// </summary>
    public List<RepositoryFolder> Folders { get; set; } = [];

    /// <summary>
    /// Gets or sets the language index.
    /// </summary>
    public LanguageIndex Languages { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration index.
    /// </summary>
    public ConfigurationIndex Configurations { get; set; } = new();

    /// <summary>
    /// Gets or sets project reference dependencies.
    /// </summary>
    public List<RepositoryDependency> Dependencies { get; set; } = [];
}
