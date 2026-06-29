namespace Jarvis.Core.Agents.Coding.Autonomous;

using Jarvis.Core.Agents.Coding.Context.Intelligent;

/// <summary>
/// Represents the final autonomous coding result.
/// </summary>
public sealed class AutonomousCodingResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the autonomous run succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether files were changed.
    /// </summary>
    public bool FilesChanged { get; set; }

    /// <summary>
    /// Gets or sets final report text.
    /// </summary>
    public string FinalReport { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets intelligent context result.
    /// </summary>
    public IntelligentContextResult ContextResult { get; set; } = new();

    /// <summary>
    /// Gets iteration results.
    /// </summary>
    public List<CodingIterationResult> Iterations { get; } = [];
}
