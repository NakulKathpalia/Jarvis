namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents the result of patch execution.
/// </summary>
public sealed class PatchResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the patch succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the patch was a dry run.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Gets or sets validation or execution messages.
    /// </summary>
    public List<string> Messages { get; set; } = [];

    /// <summary>
    /// Gets or sets the patch preview.
    /// </summary>
    public PatchPreview Preview { get; set; } = new();

    /// <summary>
    /// Gets or sets rollback history.
    /// </summary>
    public PatchHistory History { get; set; } = new();
}
