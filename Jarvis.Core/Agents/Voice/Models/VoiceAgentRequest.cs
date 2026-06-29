namespace Jarvis.Core.Agents.Voice.Models;

/// <summary>
/// Represents a generic Voice Agent request.
/// </summary>
public sealed class VoiceAgentRequest
{
    /// <summary>
    /// Gets or sets the request input.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional request metadata.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = [];
}
