namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Reviews AI-generated coding changes before apply.
/// </summary>
public sealed class CodeReviewEngine
{
    private readonly SecurityReview securityReview;
    private readonly PerformanceReview performanceReview;
    private readonly ArchitectureReview architectureReview;
    private readonly StyleReview styleReview;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeReviewEngine"/> class.
    /// </summary>
    public CodeReviewEngine()
        : this(new SecurityReview(), new PerformanceReview(), new ArchitectureReview(), new StyleReview())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeReviewEngine"/> class.
    /// </summary>
    public CodeReviewEngine(
        SecurityReview securityReview,
        PerformanceReview performanceReview,
        ArchitectureReview architectureReview,
        StyleReview styleReview)
    {
        this.securityReview = securityReview;
        this.performanceReview = performanceReview;
        this.architectureReview = architectureReview;
        this.styleReview = styleReview;
    }

    /// <summary>
    /// Reviews a coding change.
    /// </summary>
    public CodeReviewResult Review(CodeReviewRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = new CodeReviewResult();
        Add(result, securityReview.Review(request.Suggestion.PatchText));
        Add(result, performanceReview.Review(request.Suggestion.PatchText));
        Add(result, architectureReview.Review(request.Suggestion.FilesAffected));
        Add(result, styleReview.Review(request.Suggestion.PatchText));

        if (string.IsNullOrWhiteSpace(request.Suggestion.PatchText))
        {
            result.Findings.Add(new ReviewFinding
            {
                Category = "Review",
                Severity = ReviewSeverity.Info,
                Message = "No patch block was returned; review is limited to suggestion text."
            });
        }

        result.BlocksApply = result.Findings.Any(finding =>
            finding.Severity is ReviewSeverity.Error or ReviewSeverity.Critical);
        result.HasWarnings = result.Findings.Any(finding =>
            finding.Severity is ReviewSeverity.Warning or ReviewSeverity.Error or ReviewSeverity.Critical);
        return result;
    }

    private static void Add(CodeReviewResult result, IEnumerable<ReviewFinding> findings)
    {
        result.Findings.AddRange(findings);
    }
}
