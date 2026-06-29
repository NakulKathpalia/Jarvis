namespace Jarvis.Core.Agents.Voice.Models;

/// <summary>
/// Represents a generic Voice Agent result.
/// </summary>
public sealed class VoiceAgentResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the voice operation succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the result output.
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the failure message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
