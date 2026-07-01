namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Contains aggregate coding experience statistics.
/// </summary>
public sealed class ExperienceStatistics
{
    /// <summary>
    /// Gets or sets total sessions.
    /// </summary>
    public int TotalSessions { get; set; }

    /// <summary>
    /// Gets or sets successful patches.
    /// </summary>
    public int SuccessfulPatches { get; set; }

    /// <summary>
    /// Gets or sets failed patches.
    /// </summary>
    public int FailedPatches { get; set; }

    /// <summary>
    /// Gets or sets successful builds.
    /// </summary>
    public int SuccessfulBuilds { get; set; }

    /// <summary>
    /// Gets frequently modified files.
    /// </summary>
    public List<string> FrequentlyModifiedFiles { get; } = [];

    /// <summary>
    /// Gets frequently used symbols.
    /// </summary>
    public List<string> FrequentlyUsedSymbols { get; } = [];
}
