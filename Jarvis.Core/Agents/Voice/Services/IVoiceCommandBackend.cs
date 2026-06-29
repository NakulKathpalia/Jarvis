namespace Jarvis.Core.Agents.Voice.Services;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the adapter boundary for the existing voice command backend.
/// </summary>
public interface IVoiceCommandBackend
{
    /// <summary>
    /// Handles voice command detection or execution for the current execution input.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">A token that cancels command handling.</param>
    /// <returns>The voice command result.</returns>
    Task<ToolResult> HandleAsync(ExecutionContext context, CancellationToken cancellationToken = default);
}
