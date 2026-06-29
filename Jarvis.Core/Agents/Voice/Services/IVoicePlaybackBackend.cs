namespace Jarvis.Core.Agents.Voice.Services;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the adapter boundary for the existing voice playback backend.
/// </summary>
public interface IVoicePlaybackBackend
{
    /// <summary>
    /// Handles voice playback for the current execution input.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">A token that cancels playback.</param>
    /// <returns>The playback result.</returns>
    Task<ToolResult> PlayAsync(ExecutionContext context, CancellationToken cancellationToken = default);
}
