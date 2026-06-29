namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Represents a code review result.
/// </summary>
public sealed class CodeReviewResult
{
    /// <summary>
    /// Gets or sets a value indicating whether apply should be blocked.
    /// </summary>
    public bool BlocksApply { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether risky changes were detected.
    /// </summary>
    public bool HasWarnings { get; set; }

    /// <summary>
    /// Gets review findings.
    /// </summary>
    public List<ReviewFinding> Findings { get; } = [];
}
