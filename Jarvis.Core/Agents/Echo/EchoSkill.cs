namespace Jarvis.Core.Agents.Echo;

using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Test-only skill that invokes the echo tool.
/// </summary>
public sealed class EchoSkill : SkillBase
{
    private readonly EchoTool tool;

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoSkill"/> class.
    /// </summary>
    public EchoSkill()
        : this(new EchoTool())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoSkill"/> class.
    /// </summary>
    /// <param name="tool">The echo tool.</param>
    public EchoSkill(EchoTool tool)
        : base(new SkillDescriptor
        {
            Name = "EchoSkill",
            DisplayName = "Echo Skill",
            Description = "Invokes the echo tool."
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
