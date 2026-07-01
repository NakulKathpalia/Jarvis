namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Describes a repeated successful coding pattern.
/// </summary>
public sealed class SuccessPattern
{
    /// <summary>
    /// Gets or sets pattern description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets occurrence count.
    /// </summary>
    public int Count { get; set; }
}
