namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Represents recommendations from coding experience.
/// </summary>
public sealed class ExperienceResult
{
    /// <summary>
    /// Gets similar successful sessions.
    /// </summary>
    public List<ExperienceSession> SimilarSuccessfulSessions { get; } = [];

    /// <summary>
    /// Gets common failure patterns.
    /// </summary>
    public List<FailurePattern> CommonFailurePatterns { get; } = [];

    /// <summary>
    /// Gets recommended files.
    /// </summary>
    public List<string> RecommendedFiles { get; } = [];

    /// <summary>
    /// Gets recommended symbols.
    /// </summary>
    public List<string> RecommendedSymbols { get; } = [];

    /// <summary>
    /// Gets or sets recommended strategy.
    /// </summary>
    public string RecommendedStrategy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets project profile.
    /// </summary>
    public ProjectCodingProfile ProjectProfile { get; set; } = new();
}
