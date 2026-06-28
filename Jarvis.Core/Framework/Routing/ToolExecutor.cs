namespace Jarvis.Core.Framework.Routing;

using Jarvis.Core.Framework.Models;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Executes tools and converts failures into tool results.
/// </summary>
public sealed class ToolExecutor : IToolExecutor
{
    /// <inheritdoc />
    public async Task<ToolResult> ExecuteAsync(
        ITool tool,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tool);

        try
        {
            return await tool.ExecuteAsync(context, cancellationToken);
        }
        catch (Exception exception)
        {
            return new ToolResult
            {
                ToolName = tool.Name,
                Succeeded = false,
                ErrorMessage = exception.Message
            };
        }
    }
}
