namespace Jarvis.Core.Agents.Coding.AI;

/// <summary>
/// Represents a parsed local coding suggestion.
/// </summary>
public sealed class CodingSuggestion
{
    /// <summary>
    /// Gets or sets the explanation.
    /// </summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets affected files.
    /// </summary>
    public List<string> FilesAffected { get; set; } = [];

    /// <summary>
    /// Gets or sets suggested changes.
    /// </summary>
    public List<string> SuggestedChanges { get; set; } = [];

    /// <summary>
    /// Gets or sets patch text when provided.
    /// </summary>
    public string PatchText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets safety warnings.
    /// </summary>
    public List<string> SafetyWarnings { get; set; } = [];
}
