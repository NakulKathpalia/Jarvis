namespace Jarvis.Core.Agents.Echo;

using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Test-only tool that returns the original task input.
/// </summary>
public sealed class EchoTool : ToolBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EchoTool"/> class.
    /// </summary>
    public EchoTool()
        : base(new ToolDescriptor
        {
            Name = "EchoTool",
            DisplayName = "Echo Tool",
            Description = "Returns the original framework task input."
        })
    {
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Succeeded(context.Request.Input));
    }
}
