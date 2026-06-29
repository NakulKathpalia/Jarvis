namespace Jarvis.Core.Agents.Coding.Autonomous;

/// <summary>
/// Defines autonomous coding execution mode.
/// </summary>
public enum CodingExecutionMode
{
    /// <summary>
    /// Generate suggestions and previews only.
    /// </summary>
    PreviewOnly,

    /// <summary>
    /// Apply changes only when explicit approval is present.
    /// </summary>
    ApplyWithApproval
}
