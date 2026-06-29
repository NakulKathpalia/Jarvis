namespace Jarvis.Core.Agents.Voice.Models;

/// <summary>
/// Represents a generic voice transcript.
/// </summary>
public sealed class VoiceTranscript
{
    /// <summary>
    /// Gets or sets the transcript text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transcript confidence.
    /// </summary>
    public double Confidence { get; set; }
}
