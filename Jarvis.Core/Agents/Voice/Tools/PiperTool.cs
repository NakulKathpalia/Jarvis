namespace Jarvis.Core.Agents.Voice.Tools;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Wraps the existing text-to-speech backend for the tool pipeline.
/// </summary>
public sealed class PiperTool : ToolBase
{
    private readonly IPiperBackend backend;

    /// <summary>
    /// Initializes a new instance of the <see cref="PiperTool"/> class.
    /// </summary>
    /// <param name="backend">The existing text-to-speech backend adapter.</param>
    public PiperTool(IPiperBackend backend)
        : base(new ToolDescriptor
        {
            Name = "PiperTool",
            DisplayName = "Piper Tool",
            Description = "Adapts the existing text-to-speech backend."
        })
    {
        this.backend = backend;
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return backend.SpeakAsync(context, cancellationToken);
    }
}
