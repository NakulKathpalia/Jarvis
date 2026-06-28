namespace Jarvis.Core.Framework.Skills;

using Jarvis.Core.Framework.Models;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Provides a reusable base class for Jarvis tools.
/// </summary>
public abstract class ToolBase : ITool
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ToolBase"/> class.
    /// </summary>
    /// <param name="descriptor">The tool descriptor.</param>
    protected ToolBase(ToolDescriptor descriptor)
    {
        Descriptor = descriptor;
    }

    /// <summary>
    /// Gets the tool descriptor.
    /// </summary>
    public ToolDescriptor Descriptor { get; }

    /// <inheritdoc />
    public string Name => Descriptor.Name;

    /// <inheritdoc />
    public async Task<ToolResult> ExecuteAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteCoreAsync(context, cancellationToken);
        }
        catch (Exception exception)
        {
            return OnExecutionFailed(exception);
        }
    }

    /// <summary>
    /// Executes the concrete tool behavior.
    /// </summary>
    /// <param name="context">The active execution context.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The tool result.</returns>
    protected abstract Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the tool result returned when execution fails.
    /// </summary>
    /// <param name="exception">The execution exception.</param>
    /// <returns>The failed tool result.</returns>
    protected virtual ToolResult OnExecutionFailed(Exception exception)
    {
        return new ToolResult
        {
            ToolName = Name,
            Succeeded = false,
            ErrorMessage = exception.Message
        };
    }

    /// <summary>
    /// Creates a successful tool result for the current tool.
    /// </summary>
    /// <param name="output">The result output.</param>
    /// <returns>A successful tool result.</returns>
    protected ToolResult Succeeded(string output = "")
    {
        return new ToolResult
        {
            ToolName = Name,
            Succeeded = true,
            Output = output
        };
    }
}
