namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Describes a repeated coding failure pattern.
/// </summary>
public sealed class FailurePattern
{
    /// <summary>
    /// Gets or sets failure reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets occurrence count.
    /// </summary>
    public int Count { get; set; }
}
