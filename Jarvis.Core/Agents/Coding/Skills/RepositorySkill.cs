namespace Jarvis.Core.Agents.Coding.Skills;

using Jarvis.Core.Agents.Coding.Tools;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Provides repository intelligence for the Coding Agent.
/// </summary>
public sealed class RepositorySkill : SkillBase
{
    private readonly RepositoryTool repositoryTool;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositorySkill"/> class.
    /// </summary>
    public RepositorySkill()
        : this(new RepositoryTool())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositorySkill"/> class.
    /// </summary>
    /// <param name="repositoryTool">The repository tool.</param>
    public RepositorySkill(RepositoryTool repositoryTool)
        : base(new SkillDescriptor
        {
            Name = "RepositorySkill",
            DisplayName = "Repository Skill",
            Description = "Builds factual repository context."
        })
    {
        this.repositoryTool = repositoryTool;
    }

    /// <inheritdoc />
    public override Task<ToolResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default)
    {
        return ExecuteToolAsync(repositoryTool, context, toolExecutor, cancellationToken);
    }
}
