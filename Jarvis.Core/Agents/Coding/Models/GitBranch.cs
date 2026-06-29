namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents Git branch information.
/// </summary>
public sealed class GitBranch
{
    /// <summary>
    /// Gets or sets the current branch.
    /// </summary>
    public string Current { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets branch names.
    /// </summary>
    public List<string> Branches { get; set; } = [];
}
