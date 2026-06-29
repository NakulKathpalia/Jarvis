namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Represents one code review finding.
/// </summary>
public sealed class ReviewFinding
{
    /// <summary>
    /// Gets or sets the review category.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets finding severity.
    /// </summary>
    public ReviewSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets finding message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets related file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
}
