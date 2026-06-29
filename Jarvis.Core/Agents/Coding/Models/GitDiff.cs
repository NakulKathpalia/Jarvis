namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents Git diff output.
/// </summary>
public sealed class GitDiff
{
    /// <summary>
    /// Gets or sets diff text.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
