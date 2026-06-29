namespace Jarvis.Core.Agents.Voice.Skills;

using Jarvis.Core.Agents.Voice.Tools;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Coordinates wake word checks through the voice tool pipeline.
/// </summary>
public sealed class WakeWordSkill : SkillBase
{
    private readonly WakeWordTool tool;

    /// <summary>
    /// Initializes a new instance of the <see cref="WakeWordSkill"/> class.
    /// </summary>
    /// <param name="tool">The wake word tool.</param>
    public WakeWordSkill(WakeWordTool tool)
        : base(new SkillDescriptor
        {
            Name = "WakeWordSkill",
            DisplayName = "Wake Word Skill",
            Description = "Delegates wake word checks to the configured wake word tool."
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
