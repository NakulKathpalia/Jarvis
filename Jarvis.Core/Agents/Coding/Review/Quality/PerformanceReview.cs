namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Performs rule-based performance review.
/// </summary>
public sealed class PerformanceReview
{
    /// <summary>
    /// Reviews patch text for performance concerns.
    /// </summary>
    public IEnumerable<ReviewFinding> Review(string patchText)
    {
        var text = patchText ?? string.Empty;
        if (text.Contains("Thread.Sleep", StringComparison.OrdinalIgnoreCase) ||
            text.Contains(".Result", StringComparison.Ordinal))
        {
            yield return new ReviewFinding
            {
                Category = "Performance",
                Severity = ReviewSeverity.Warning,
                Message = "Patch may block threads or async execution."
            };
        }
    }
}
