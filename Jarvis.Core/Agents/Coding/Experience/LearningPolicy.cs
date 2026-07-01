namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Controls how coding experience is retained and queried.
/// </summary>
public sealed class LearningPolicy
{
    /// <summary>
    /// Gets or sets a value indicating whether failed sessions should be retained.
    /// </summary>
    public bool TrackFailures { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether prompts should be retained.
    /// </summary>
    public bool StorePrompts { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of similar sessions returned.
    /// </summary>
    public int MaxSimilarSessions { get; set; } = 5;
}
