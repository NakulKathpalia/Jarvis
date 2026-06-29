namespace Jarvis.Core.Agents.Voice.Tools;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Wraps the existing wake word backend for the tool pipeline.
/// </summary>
public sealed class WakeWordTool : ToolBase
{
    private readonly IWakeWordBackend backend;

    /// <summary>
    /// Initializes a new instance of the <see cref="WakeWordTool"/> class.
    /// </summary>
    /// <param name="backend">The existing wake word backend adapter.</param>
    public WakeWordTool(IWakeWordBackend backend)
        : base(new ToolDescriptor
        {
            Name = "WakeWordTool",
            DisplayName = "Wake Word Tool",
            Description = "Adapts the existing wake word backend."
        })
    {
        this.backend = backend;
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return backend.CheckAsync(context, cancellationToken);
    }
}
