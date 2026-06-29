namespace Jarvis.Core.Agents.Coding.Assistant;

/// <summary>
/// Represents local coding assistant modes.
/// </summary>
public enum CodingAssistantMode
{
    /// <summary>
    /// Explain context and approach only.
    /// </summary>
    ExplainOnly,

    /// <summary>
    /// Suggest code changes without patch formatting.
    /// </summary>
    SuggestCode,

    /// <summary>
    /// Suggest a patch preview without applying it.
    /// </summary>
    SuggestPatch,

    /// <summary>
    /// Suggest a minimal build fix from compiler errors.
    /// </summary>
    BuildFix,

    /// <summary>
    /// Run autonomous preview without applying files.
    /// </summary>
    AutonomousPreview,

    /// <summary>
    /// Run autonomous apply only when explicit approval is present.
    /// </summary>
    AutonomousApplyWithApproval,

    /// <summary>
    /// Review a proposed change only.
    /// </summary>
    ReviewOnly,

    /// <summary>
    /// Run sequential multi-agent preview.
    /// </summary>
    MultiAgentPreview
}
