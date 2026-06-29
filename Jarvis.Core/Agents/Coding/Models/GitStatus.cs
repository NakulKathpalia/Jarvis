namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents Git status output.
/// </summary>
public sealed class GitStatus
{
    /// <summary>
    /// Gets or sets status lines.
    /// </summary>
    public List<string> Lines { get; set; } = [];
}
