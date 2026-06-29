namespace Jarvis.Core.Agents.Voice.Skills;

using Jarvis.Core.Agents.Voice.Tools;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Coordinates voice command handling through the voice tool pipeline.
/// </summary>
public sealed class VoiceCommandSkill : SkillBase
{
    private readonly VoiceCommandTool tool;

    /// <summary>
    /// Initializes a new instance of the <see cref="VoiceCommandSkill"/> class.
    /// </summary>
    /// <param name="tool">The voice command tool.</param>
    public VoiceCommandSkill(VoiceCommandTool tool)
        : base(new SkillDescriptor
        {
            Name = "VoiceCommandSkill",
            DisplayName = "Voice Command Skill",
            Description = "Delegates command-facing voice handling to the configured command tool."
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
