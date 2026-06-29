namespace Jarvis.Core.Agents.Voice.Tools;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Wraps the existing speech-to-text backend for the tool pipeline.
/// </summary>
public sealed class WhisperTool : ToolBase
{
    private readonly IWhisperBackend backend;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperTool"/> class.
    /// </summary>
    /// <param name="backend">The existing speech-to-text backend adapter.</param>
    public WhisperTool(IWhisperBackend backend)
        : base(new ToolDescriptor
        {
            Name = "WhisperTool",
            DisplayName = "Whisper Tool",
            Description = "Adapts the existing speech-to-text backend."
        })
    {
        this.backend = backend;
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return backend.TranscribeAsync(context, cancellationToken);
    }
}
