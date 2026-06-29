namespace Jarvis.Core.Agents.Voice.Tools;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Wraps the existing voice command backend for the tool pipeline.
/// </summary>
public sealed class VoiceCommandTool : ToolBase
{
    private readonly IVoiceCommandBackend backend;

    /// <summary>
    /// Initializes a new instance of the <see cref="VoiceCommandTool"/> class.
    /// </summary>
    /// <param name="backend">The existing voice command backend adapter.</param>
    public VoiceCommandTool(IVoiceCommandBackend backend)
        : base(new ToolDescriptor
        {
            Name = "VoiceCommandTool",
            DisplayName = "Voice Command Tool",
            Description = "Adapts the existing voice command backend."
        })
    {
        this.backend = backend;
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return backend.HandleAsync(context, cancellationToken);
    }
}
