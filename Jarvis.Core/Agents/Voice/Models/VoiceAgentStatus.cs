namespace Jarvis.Core.Agents.Voice.Models;

/// <summary>
/// Represents framework-level Voice Agent status.
/// </summary>
public sealed class VoiceAgentStatus
{
    /// <summary>
    /// Gets or sets the current Voice Agent state.
    /// </summary>
    public string CurrentState { get; set; } = "Idle";

    /// <summary>
    /// Gets or sets the Voice Agent health.
    /// </summary>
    public string Health { get; set; } = "Unknown";

    /// <summary>
    /// Gets or sets the active task identifier.
    /// </summary>
    public string ActiveTask { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string LastError { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last execution time.
    /// </summary>
    public DateTimeOffset? LastExecutionTimeUtc { get; set; }

    /// <summary>
    /// Gets or sets the current pipeline state.
    /// </summary>
    public string CurrentPipelineState { get; set; } = "Idle";
}
