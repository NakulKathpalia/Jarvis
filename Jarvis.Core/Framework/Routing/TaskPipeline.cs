namespace Jarvis.Core.Framework.Routing;

using Jarvis.Core.Framework.Agents;
using Jarvis.Core.Framework.Events;
using Jarvis.Core.Framework.Models;

/// <summary>
/// Provides the main task execution pipeline for the Jarvis framework.
/// </summary>
public sealed class TaskPipeline : ITaskPipeline
{
    private readonly IAgentManager agentManager;
    private readonly IEventBus? eventBus;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPipeline"/> class.
    /// </summary>
    /// <param name="agentManager">The agent manager.</param>
    /// <param name="eventBus">The optional event bus.</param>
    public TaskPipeline(IAgentManager agentManager, IEventBus? eventBus = null)
    {
        this.agentManager = agentManager;
        this.eventBus = eventBus;
    }

    /// <inheritdoc />
    public async Task<TaskResult> ExecuteAsync(TaskRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await PublishAsync("TaskStarted", request.RequestId, request, cancellationToken);
        var result = await agentManager.ExecuteAsync(request, cancellationToken);
        await PublishAsync(result.Succeeded ? "TaskCompleted" : "TaskFailed", request.RequestId, result, cancellationToken);

        return result;
    }

    private Task PublishAsync(
        string name,
        string correlationId,
        object payload,
        CancellationToken cancellationToken)
    {
        if (eventBus is null)
        {
            return Task.CompletedTask;
        }

        return eventBus.PublishAsync(new FrameworkEvent
        {
            Name = name,
            CorrelationId = correlationId,
            Payload = payload
        }, cancellationToken);
    }
}
