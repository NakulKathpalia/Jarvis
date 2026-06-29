namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a Git commit entry.
/// </summary>
public sealed class GitCommit
{
    /// <summary>
    /// Gets or sets the commit hash.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
}
