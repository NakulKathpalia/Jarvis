namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a read-only coding planning strategy.
/// </summary>
public enum PlanningStrategy
{
    /// <summary>
    /// Read relevant repository areas.
    /// </summary>
    Read,

    /// <summary>
    /// Locate implementation points.
    /// </summary>
    Locate,

    /// <summary>
    /// Prepare candidate patch targets without editing.
    /// </summary>
    PreparePatchTargets
}
