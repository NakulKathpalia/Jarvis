namespace Jarvis.Core.Agents.Voice.Skills;

using Jarvis.Core.Agents.Voice.Tools;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Coordinates text-to-speech execution through the voice tool pipeline.
/// </summary>
public sealed class SpeakSkill : SkillBase
{
    private readonly VoicePlaybackTool tool;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpeakSkill"/> class.
    /// </summary>
    /// <param name="tool">The voice playback tool.</param>
    public SpeakSkill(VoicePlaybackTool tool)
        : base(new SkillDescriptor
        {
            Name = "SpeakSkill",
            DisplayName = "Speak Skill",
            Description = "Delegates speech generation and playback to the configured voice playback tool."
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
