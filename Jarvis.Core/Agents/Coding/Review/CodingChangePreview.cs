namespace Jarvis.Core.Agents.Coding.Review;

/// <summary>
/// Represents a preview of suggested code changes.
/// </summary>
public sealed class CodingChangePreview
{
    /// <summary>
    /// Gets or sets the change summary.
    /// </summary>
    public CodingChangeSummary Summary { get; set; } = new();

    /// <summary>
    /// Gets or sets patch text when present.
    /// </summary>
    public string PatchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether user approval is required before applying changes.
    /// </summary>
    public bool RequiresApproval { get; set; } = true;
}
