namespace Jarvis.Core.Agents.Voice.Skills;

using Jarvis.Core.Agents.Voice.Tools;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Coordinates playback-facing voice listening flow through the voice tool pipeline.
/// </summary>
public sealed class ListenSkill : SkillBase
{
    private readonly VoicePipelineTool tool;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenSkill"/> class.
    /// </summary>
    /// <param name="tool">The voice pipeline tool.</param>
    public ListenSkill(VoicePipelineTool tool)
        : base(new SkillDescriptor
        {
            Name = "ListenSkill",
            DisplayName = "Listen Skill",
            Description = "Delegates listening-related processing to the configured voice pipeline tool."
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
