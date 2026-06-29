namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Performs rule-based style review.
/// </summary>
public sealed class StyleReview
{
    /// <summary>
    /// Reviews patch text for simple style issues.
    /// </summary>
    public IEnumerable<ReviewFinding> Review(string patchText)
    {
        if ((patchText ?? string.Empty).Length > 20000)
        {
            yield return new ReviewFinding
            {
                Category = "Style",
                Severity = ReviewSeverity.Warning,
                Message = "Patch preview is large; prefer smaller focused changes."
            };
        }
    }
}
