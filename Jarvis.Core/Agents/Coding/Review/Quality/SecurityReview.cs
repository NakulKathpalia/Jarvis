namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Performs rule-based security review.
/// </summary>
public sealed class SecurityReview
{
    /// <summary>
    /// Reviews patch text for security-sensitive operations.
    /// </summary>
    public IEnumerable<ReviewFinding> Review(string patchText)
    {
        var text = patchText ?? string.Empty;
        if (text.Contains("Process.Start", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("cmd.exe", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("powershell", StringComparison.OrdinalIgnoreCase))
        {
            yield return Finding(ReviewSeverity.Critical, "Patch introduces process execution and requires manual security approval.");
        }

        if (text.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("apiKey", StringComparison.OrdinalIgnoreCase))
        {
            yield return Finding(ReviewSeverity.Warning, "Patch references credentials or secrets.");
        }
    }

    private static ReviewFinding Finding(ReviewSeverity severity, string message)
    {
        return new ReviewFinding { Category = "Security", Severity = severity, Message = message };
    }
}
