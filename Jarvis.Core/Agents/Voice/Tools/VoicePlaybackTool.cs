namespace Jarvis.Core.Agents.Voice.Tools;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Wraps the existing voice playback backend for the tool pipeline.
/// </summary>
public sealed class VoicePlaybackTool : ToolBase
{
    private readonly IVoicePlaybackBackend backend;

    /// <summary>
    /// Initializes a new instance of the <see cref="VoicePlaybackTool"/> class.
    /// </summary>
    /// <param name="backend">The existing playback backend adapter.</param>
    public VoicePlaybackTool(IVoicePlaybackBackend backend)
        : base(new ToolDescriptor
        {
            Name = "VoicePlaybackTool",
            DisplayName = "Voice Playback Tool",
            Description = "Adapts the existing playback backend."
        })
    {
        this.backend = backend;
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return backend.PlayAsync(context, cancellationToken);
    }
}
