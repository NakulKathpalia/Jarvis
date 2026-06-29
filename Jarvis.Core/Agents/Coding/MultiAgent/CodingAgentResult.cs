namespace Jarvis.Core.Agents.Coding.MultiAgent;

using Jarvis.Core.Agents.Coding.Autonomous;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Represents a role result in the coding orchestrator.
/// </summary>
public sealed class CodingAgentResult
{
    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public CodingAgentRole Role { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the role succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets role output.
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets autonomous result when produced.
    /// </summary>
    public AutonomousCodingResult? AutonomousResult { get; set; }

    /// <summary>
    /// Gets or sets review result when produced.
    /// </summary>
    public CodeReviewResult? ReviewResult { get; set; }

    /// <summary>
    /// Gets or sets build result when produced.
    /// </summary>
    public BuildResult? BuildResult { get; set; }
}
