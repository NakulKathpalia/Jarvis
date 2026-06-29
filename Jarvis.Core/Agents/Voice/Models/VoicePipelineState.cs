namespace Jarvis.Core.Agents.Voice.Models;

/// <summary>
/// Defines generic Voice Agent pipeline states.
/// </summary>
public enum VoicePipelineState
{
    /// <summary>
    /// The voice pipeline is idle.
    /// </summary>
    Idle,

    /// <summary>
    /// The voice pipeline is listening.
    /// </summary>
    Listening,

    /// <summary>
    /// The voice pipeline is transcribing audio.
    /// </summary>
    Transcribing,

    /// <summary>
    /// The voice pipeline is checking a wake word.
    /// </summary>
    WakeWordChecking,

    /// <summary>
    /// The voice pipeline is handling a command.
    /// </summary>
    CommandHandling,

    /// <summary>
    /// The voice pipeline is speaking.
    /// </summary>
    Speaking,

    /// <summary>
    /// The voice pipeline completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The voice pipeline failed.
    /// </summary>
    Error
}
