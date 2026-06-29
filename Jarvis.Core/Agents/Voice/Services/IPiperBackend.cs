namespace Jarvis.Core.Agents.Voice.Services;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the adapter boundary for the existing text-to-speech backend.
/// </summary>
public interface IPiperBackend
{
    /// <summary>
    /// Generates speech for the current execution input.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">A token that cancels speech generation.</param>
    /// <returns>The speech generation result.</returns>
    Task<ToolResult> SpeakAsync(ExecutionContext context, CancellationToken cancellationToken = default);
}
