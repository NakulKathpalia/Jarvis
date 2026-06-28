namespace Jarvis.Core.Framework.Agents;

using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Provides a reusable base class for Jarvis agents.
/// </summary>
public abstract class AgentBase : IAgent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentBase"/> class.
    /// </summary>
    /// <param name="descriptor">The agent descriptor.</param>
    protected AgentBase(AgentDescriptor descriptor)
    {
        Descriptor = descriptor;
    }

    /// <summary>
    /// Gets the agent descriptor.
    /// </summary>
    public AgentDescriptor Descriptor { get; }

    /// <inheritdoc />
    public string Name => Descriptor.Name;

    /// <inheritdoc />
    public abstract Task<TaskResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a successful task result for the current agent.
    /// </summary>
    /// <param name="context">The active agent context.</param>
    /// <param name="output">The result output.</param>
    /// <returns>A successful task result.</returns>
    protected TaskResult Succeeded(AgentContext context, string output = "")
    {
        return new TaskResult
        {
            RequestId = context.ExecutionContext.Request.RequestId,
            AgentName = Name,
            Succeeded = true,
            Output = output
        };
    }

    /// <summary>
    /// Creates a failed task result for the current agent.
    /// </summary>
    /// <param name="context">The active agent context.</param>
    /// <param name="errorMessage">The failure message.</param>
    /// <returns>A failed task result.</returns>
    protected TaskResult Failed(AgentContext context, string errorMessage)
    {
        return new TaskResult
        {
            RequestId = context.ExecutionContext.Request.RequestId,
            AgentName = Name,
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}
