namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a configuration file discovered in a repository.
/// </summary>
public sealed class RepositoryConfiguration
{
    /// <summary>
    /// Gets or sets the configuration type.
    /// </summary>
    public string ConfigurationType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository-relative file path.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;
}
