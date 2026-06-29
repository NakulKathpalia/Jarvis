namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a local Git repository.
/// </summary>
public sealed class GitRepository
{
    /// <summary>
    /// Gets or sets the repository path.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
