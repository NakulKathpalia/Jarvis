namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents Git commit history.
/// </summary>
public sealed class GitHistory
{
    /// <summary>
    /// Gets or sets commits.
    /// </summary>
    public List<GitCommit> Commits { get; set; } = [];
}
