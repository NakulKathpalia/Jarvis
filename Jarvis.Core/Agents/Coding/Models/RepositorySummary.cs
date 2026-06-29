namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a factual repository summary.
/// </summary>
public sealed class RepositorySummary
{
    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets detected languages.
    /// </summary>
    public List<string> Languages { get; set; } = [];

    /// <summary>
    /// Gets or sets detected configuration types.
    /// </summary>
    public List<string> Configurations { get; set; } = [];

    /// <summary>
    /// Gets or sets the dependency summary.
    /// </summary>
    public List<string> DependencySummary { get; set; } = [];
}
