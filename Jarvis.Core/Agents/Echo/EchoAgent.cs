namespace Jarvis.Core.Agents.Echo;

using Jarvis.Core.Framework.Agents;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Test-only agent that validates the Agent to Skill to Tool framework path.
/// </summary>
public sealed class EchoAgent : AgentBase
{
    private readonly EchoSkill skill;

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoAgent"/> class.
    /// </summary>
    public EchoAgent()
        : this(new EchoSkill())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EchoAgent"/> class.
    /// </summary>
    /// <param name="skill">The echo skill.</param>
    public EchoAgent(EchoSkill skill)
        : base(new AgentDescriptor
        {
            Name = "EchoAgent",
            DisplayName = "Echo Agent",
            Description = "Validates the framework runtime pipeline.",
            SupportedTaskTypes = ["Echo"]
        })
    {
        this.skill = skill;
    }

    /// <inheritdoc />
    public override async Task<TaskResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default)
    {
        var toolResult = await skill.ExecuteAsync(context, toolExecutor, cancellationToken);

        return new TaskResult
        {
            RequestId = context.ExecutionContext.Request.RequestId,
            AgentName = Name,
            Succeeded = toolResult.Succeeded,
            Output = toolResult.Output,
            ErrorMessage = toolResult.ErrorMessage,
            ToolResults = [toolResult]
        };
    }
}
