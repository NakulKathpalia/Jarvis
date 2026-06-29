namespace Jarvis.Core.Agents.Coding.Autonomous;

using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Review;
using Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Represents the result of one autonomous coding iteration.
/// </summary>
public sealed class CodingIterationResult
{
    /// <summary>
    /// Gets or sets iteration metadata.
    /// </summary>
    public CodingIteration Iteration { get; set; } = new();

    /// <summary>
    /// Gets or sets the model response.
    /// </summary>
    public CodingModelResponse ModelResponse { get; set; } = new();

    /// <summary>
    /// Gets or sets parsed suggestion.
    /// </summary>
    public CodingSuggestion Suggestion { get; set; } = new();

    /// <summary>
    /// Gets or sets change preview.
    /// </summary>
    public CodingChangePreview ChangePreview { get; set; } = new();

    /// <summary>
    /// Gets or sets review result.
    /// </summary>
    public CodeReviewResult ReviewResult { get; set; } = new();

    /// <summary>
    /// Gets or sets patch result when a patch was applied.
    /// </summary>
    public PatchResult? PatchResult { get; set; }

    /// <summary>
    /// Gets or sets build result when a build was run.
    /// </summary>
    public BuildResult? BuildResult { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether files were changed.
    /// </summary>
    public bool FilesChanged { get; set; }
}
