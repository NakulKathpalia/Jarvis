namespace Jarvis.Core.Agents.Coding.Autonomous;

/// <summary>
/// Describes one autonomous coding iteration.
/// </summary>
public sealed class CodingIteration
{
    /// <summary>
    /// Gets or sets the iteration number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets iteration purpose.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;
}
