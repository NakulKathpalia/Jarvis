namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Performs rule-based architecture review.
/// </summary>
public sealed class ArchitectureReview
{
    /// <summary>
    /// Reviews affected files for architecture risk.
    /// </summary>
    public IEnumerable<ReviewFinding> Review(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            if (file.Contains("Program.cs", StringComparison.OrdinalIgnoreCase) ||
                file.Contains("Startup", StringComparison.OrdinalIgnoreCase))
            {
                yield return new ReviewFinding
                {
                    Category = "Architecture",
                    Severity = ReviewSeverity.Warning,
                    Message = "Patch touches application startup or host configuration.",
                    FilePath = file
                };
            }
        }
    }
}
