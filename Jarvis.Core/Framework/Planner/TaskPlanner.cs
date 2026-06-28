namespace Jarvis.Core.Framework.Planner;

using Jarvis.Core.Core.Exceptions;
using Jarvis.Core.Framework.Models;

/// <summary>
/// Provides rule-based agent selection for framework task requests.
/// </summary>
public sealed class TaskPlanner : ITaskPlanner
{
    private readonly IReadOnlyDictionary<string, string> agentMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPlanner"/> class.
    /// </summary>
    public TaskPlanner()
        : this(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Voice"] = "Voice Agent",
            ["Coding"] = "Coding Agent",
            ["Memory"] = "Memory Agent",
            ["Vision"] = "Vision Agent",
            ["Automation"] = "Automation Agent",
            ["Browser"] = "Browser Agent",
            ["Research"] = "Research Agent",
            ["Security"] = "Security Agent",
            ["Knowledge"] = "Knowledge Agent"
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPlanner"/> class.
    /// </summary>
    /// <param name="agentMap">The task type to agent name map.</param>
    public TaskPlanner(IReadOnlyDictionary<string, string> agentMap)
    {
        this.agentMap = agentMap;
    }

    /// <inheritdoc />
    public Task<string> SelectAgentAsync(
        TaskRequest request,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TaskType))
        {
            throw new PlannerException("Task type is required.");
        }

        if (!agentMap.TryGetValue(request.TaskType, out var agentName))
        {
            throw new PlannerException($"No agent rule exists for task type: {request.TaskType}");
        }

        return Task.FromResult(agentName);
    }
}
