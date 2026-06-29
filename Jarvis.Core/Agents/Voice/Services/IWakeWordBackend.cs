namespace Jarvis.Core.Agents.Voice.Services;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the adapter boundary for the existing wake word backend.
/// </summary>
public interface IWakeWordBackend
{
    /// <summary>
    /// Checks wake word status or detection for the current execution input.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">A token that cancels wake word checking.</param>
    /// <returns>The wake word result.</returns>
    Task<ToolResult> CheckAsync(ExecutionContext context, CancellationToken cancellationToken = default);
}
