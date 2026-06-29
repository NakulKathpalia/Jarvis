namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a build request.
/// </summary>
public sealed class BuildRequest
{
    /// <summary>
    /// Gets or sets the repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional build configuration.
    /// </summary>
    public BuildConfiguration? Configuration { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether restore should be skipped when supported.
    /// </summary>
    public bool NoRestore { get; set; }
}
