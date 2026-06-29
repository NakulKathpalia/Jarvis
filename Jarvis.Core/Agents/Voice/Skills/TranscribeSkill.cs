namespace Jarvis.Core.Agents.Voice.Skills;

using Jarvis.Core.Agents.Voice.Tools;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Coordinates speech-to-text execution through the voice tool pipeline.
/// </summary>
public sealed class TranscribeSkill : SkillBase
{
    private readonly WhisperTool tool;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscribeSkill"/> class.
    /// </summary>
    /// <param name="tool">The whisper tool.</param>
    public TranscribeSkill(WhisperTool tool)
        : base(new SkillDescriptor
        {
            Name = "TranscribeSkill",
            DisplayName = "Transcribe Skill",
            Description = "Delegates transcription to the configured speech-to-text tool."
        })
    {
        this.tool = tool;
    }

    /// <inheritdoc />
    public override Task<ToolResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(tool, context, toolExecutor, cancellationToken);
    }
}
