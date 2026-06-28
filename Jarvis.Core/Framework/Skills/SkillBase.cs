namespace Jarvis.Core.Framework.Skills;

using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Provides a reusable base class for Jarvis skills.
/// </summary>
public abstract class SkillBase : ISkill
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkillBase"/> class.
    /// </summary>
    /// <param name="descriptor">The skill descriptor.</param>
    protected SkillBase(SkillDescriptor descriptor)
    {
        Descriptor = descriptor;
    }

    /// <summary>
    /// Gets the skill descriptor.
    /// </summary>
    public SkillDescriptor Descriptor { get; }

    /// <inheritdoc />
    public string Name => Descriptor.Name;

    /// <inheritdoc />
    public abstract Task<ToolResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool through the framework tool executor.
    /// </summary>
    /// <param name="tool">The tool to execute.</param>
    /// <param name="context">The active agent context.</param>
    /// <param name="toolExecutor">The framework tool executor.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The tool result.</returns>
    protected Task<ToolResult> ExecuteToolAsync(
        ITool tool,
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default)
    {
        return toolExecutor.ExecuteAsync(tool, context.ExecutionContext, cancellationToken);
    }
}
