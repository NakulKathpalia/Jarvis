namespace Jarvis.Core.Agents.Coding.Review;

/// <summary>
/// Represents a high-level summary of suggested code changes.
/// </summary>
public sealed class CodingChangeSummary
{
    /// <summary>
    /// Gets or sets affected files.
    /// </summary>
    public List<string> FilesAffected { get; set; } = [];

    /// <summary>
    /// Gets or sets suggested changes.
    /// </summary>
    public List<string> SuggestedChanges { get; set; } = [];

    /// <summary>
    /// Gets or sets safety warnings.
    /// </summary>
    public List<string> SafetyWarnings { get; set; } = [];
}
