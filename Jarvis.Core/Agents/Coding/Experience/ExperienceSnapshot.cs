namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Represents a snapshot of stored engineering experience.
/// </summary>
public sealed class ExperienceSnapshot
{
    /// <summary>
    /// Gets sessions in the snapshot.
    /// </summary>
    public List<ExperienceSession> Sessions { get; } = [];

    /// <summary>
    /// Gets entries in the snapshot.
    /// </summary>
    public List<ExperienceEntry> Entries { get; } = [];
}
