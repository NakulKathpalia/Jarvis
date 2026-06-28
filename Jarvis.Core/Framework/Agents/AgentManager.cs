namespace Jarvis.Core.Framework.Agents;

using Jarvis.Core.Core.Exceptions;
using Jarvis.Core.Core.Logging;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Planner;
using Jarvis.Core.Framework.Registry;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Coordinates planning, agent resolution, and agent execution.
/// </summary>
public sealed class AgentManager : IAgentManager
{
    private readonly ITaskPlanner planner;
    private readonly IAgentRegistry registry;
    private readonly IContextProvider contextProvider;
    private readonly IToolExecutor toolExecutor;
    private readonly IFrameworkLogger? logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentManager"/> class.
    /// </summary>
    public AgentManager(
        ITaskPlanner planner,
        IAgentRegistry registry,
        IContextProvider contextProvider,
        IToolExecutor toolExecutor,
        IFrameworkLogger? logger = null)
    {
        this.planner = planner;
        this.registry = registry;
        this.contextProvider = contextProvider;
        this.toolExecutor = toolExecutor;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<TaskResult> ExecuteAsync(TaskRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var context = contextProvider.CreateContext(request);

        try
        {
            var agentName = await planner.SelectAgentAsync(request, context, cancellationToken);
            context.AgentName = agentName;

            var agent = registry.GetByName(agentName) ?? throw new AgentNotFoundException(agentName);
            var agentContext = new AgentContext
            {
                AgentName = agentName,
                ExecutionContext = context
            };

            await LogAsync(FrameworkLogLevel.Information, "Agent execution started.", context, null, cancellationToken);
            var result = await agent.ExecuteAsync(agentContext, toolExecutor, cancellationToken);
            result.RequestId = string.IsNullOrWhiteSpace(result.RequestId) ? request.RequestId : result.RequestId;
            result.AgentName = string.IsNullOrWhiteSpace(result.AgentName) ? agentName : result.AgentName;
            await LogAsync(FrameworkLogLevel.Information, "Agent execution completed.", context, null, cancellationToken);

            return result;
        }
        catch (Exception exception)
        {
            await LogAsync(FrameworkLogLevel.Error, "Agent execution failed.", context, exception, cancellationToken);
            return new TaskResult
            {
                RequestId = request.RequestId,
                AgentName = context.AgentName,
                Succeeded = false,
                ErrorMessage = exception.Message
            };
        }
    }

    private Task LogAsync(
        FrameworkLogLevel level,
        string message,
        ExecutionContext context,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        if (logger is null)
        {
            return Task.CompletedTask;
        }

        return logger.LogAsync(new FrameworkLogEntry
        {
            Level = level,
            Message = message,
            CorrelationId = context.CorrelationId,
            Exception = exception
        }, cancellationToken);
    }
}
