namespace Jarvis.Core.Framework.Routing;

using Jarvis.Core.Framework.Models;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Defines a component that executes tools and returns tool results.
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// Executes a tool for the supplied execution context.
    /// </summary>
    /// <param name="tool">The tool to execute.</param>
    /// <param name="context">The active execution context.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The tool result.</returns>
    Task<ToolResult> ExecuteAsync(
        ITool tool,
        ExecutionContext context,
        CancellationToken cancellationToken = default);
}
