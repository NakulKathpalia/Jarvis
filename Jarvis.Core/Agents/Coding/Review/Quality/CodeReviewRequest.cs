namespace Jarvis.Core.Agents.Coding.Review.Quality;

using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Review;

/// <summary>
/// Represents a request to review a coding change.
/// </summary>
public sealed class CodeReviewRequest
{
    /// <summary>
    /// Gets or sets the user request.
    /// </summary>
    public string UserRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parsed suggestion.
    /// </summary>
    public CodingSuggestion Suggestion { get; set; } = new();

    /// <summary>
    /// Gets or sets the change preview.
    /// </summary>
    public CodingChangePreview ChangePreview { get; set; } = new();
}
