namespace Jarvis.Core.Agents.Voice.Tools;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Wraps the existing end-to-end voice pipeline backend for the tool pipeline.
/// </summary>
public sealed class VoicePipelineTool : ToolBase
{
    private readonly IVoicePipelineBackend backend;

    /// <summary>
    /// Initializes a new instance of the <see cref="VoicePipelineTool"/> class.
    /// </summary>
    /// <param name="backend">The existing voice pipeline backend adapter.</param>
    public VoicePipelineTool(IVoicePipelineBackend backend)
        : base(new ToolDescriptor
        {
            Name = "VoicePipelineTool",
            DisplayName = "Voice Pipeline Tool",
            Description = "Adapts the existing voice pipeline backend."
        })
    {
        this.backend = backend;
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return backend.ProcessAsync(context, cancellationToken);
    }
}
