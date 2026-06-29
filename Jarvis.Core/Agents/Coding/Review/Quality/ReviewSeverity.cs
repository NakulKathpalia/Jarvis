namespace Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Defines review finding severity.
/// </summary>
public enum ReviewSeverity
{
    /// <summary>
    /// Informational finding.
    /// </summary>
    Info,

    /// <summary>
    /// Warning finding.
    /// </summary>
    Warning,

    /// <summary>
    /// Error finding that should block apply.
    /// </summary>
    Error,

    /// <summary>
    /// Critical finding that must block apply.
    /// </summary>
    Critical
}
