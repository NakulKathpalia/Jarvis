namespace Jarvis.Core.Agents.Voice.Services;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines the adapter boundary for the existing voice pipeline backend.
/// </summary>
public interface IVoicePipelineBackend
{
    /// <summary>
    /// Processes a voice pipeline operation using the existing backend.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">A token that cancels processing.</param>
    /// <returns>The voice pipeline tool result.</returns>
    Task<ToolResult> ProcessAsync(ExecutionContext context, CancellationToken cancellationToken = default);
}
