namespace Jarvis.Core.Agents.Voice.Services;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the adapter boundary for the existing speech-to-text backend.
/// </summary>
public interface IWhisperBackend
{
    /// <summary>
    /// Transcribes the current execution input.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">A token that cancels transcription.</param>
    /// <returns>The transcription result.</returns>
    Task<ToolResult> TranscribeAsync(ExecutionContext context, CancellationToken cancellationToken = default);
}
