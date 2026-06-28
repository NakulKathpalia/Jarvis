namespace Jarvis.Core.Shared.Interfaces;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Defines an executable tool used by Jarvis skills.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the unique tool name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the tool for the supplied execution context.
    /// </summary>
    /// <param name="context">The current execution context.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The tool execution result.</returns>
    Task<ToolResult> ExecuteAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default);
}
