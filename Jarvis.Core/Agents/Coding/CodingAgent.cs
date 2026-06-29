namespace Jarvis.Core.Agents.Coding;

using Jarvis.Core.Agents.Coding.Skills;
using Jarvis.Core.Framework.Agents;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Provides read-only repository intelligence for coding workflows.
/// </summary>
public sealed class CodingAgent : AgentBase
{
    private readonly RepositorySkill repositorySkill;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodingAgent"/> class.
    /// </summary>
    public CodingAgent()
        : this(new RepositorySkill())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodingAgent"/> class.
    /// </summary>
    /// <param name="repositorySkill">The repository skill.</param>
    public CodingAgent(RepositorySkill repositorySkill)
        : base(new AgentDescriptor
        {
            Name = "Coding Agent",
            DisplayName = "Coding Agent",
            Description = "Reads repositories and builds factual repository context.",
            Version = "1.0.0",
            SupportedTaskTypes = ["Coding"]
        })
    {
        this.repositorySkill = repositorySkill;
    }

    /// <inheritdoc />
    public override async Task<TaskResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default)
    {
        var toolResult = await repositorySkill.ExecuteAsync(context, toolExecutor, cancellationToken);
        if (!toolResult.Succeeded)
        {
            return Failed(context, toolResult.ErrorMessage);
        }

        var result = Succeeded(context, toolResult.Output);
        result.ToolResults.Add(toolResult);
        return result;
    }
}
