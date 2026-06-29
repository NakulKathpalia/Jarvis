namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a Git command result.
/// </summary>
public sealed class GitOperationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the command succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the exit code.
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    /// Gets or sets command output.
    /// </summary>
    public string Output { get; set; } = string.Empty;
}
