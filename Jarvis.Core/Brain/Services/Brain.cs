namespace Jarvis.Core.Brain.Services;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;
using Jarvis.Core.Core.Logging;
using Jarvis.Core.Framework.Registry;
using Jarvis.Core.Framework.Skills;
using Jarvis.Core.Shared.Interfaces;

/// <summary>
/// Coordinates Brain analysis and planning without executing the plan.
/// </summary>
public sealed class Brain : IBrain
{
    private readonly IIntentAnalyzer intentAnalyzer;
    private readonly ITaskAnalyzer taskAnalyzer;
    private readonly IWorkflowPlanner workflowPlanner;
    private readonly IAgentSelector agentSelector;
    private readonly ISkillSelector skillSelector;
    private readonly IToolSelector toolSelector;
    private readonly IModelRouter modelRouter;
    private readonly IFrameworkLogger? logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Brain"/> class.
    /// </summary>
    public Brain(
        IIntentAnalyzer intentAnalyzer,
        ITaskAnalyzer taskAnalyzer,
        IWorkflowPlanner workflowPlanner,
        IAgentSelector agentSelector,
        ISkillSelector skillSelector,
        IToolSelector toolSelector,
        IModelRouter modelRouter,
        IFrameworkLogger? logger = null)
    {
        this.intentAnalyzer = intentAnalyzer;
        this.taskAnalyzer = taskAnalyzer;
        this.workflowPlanner = workflowPlanner;
        this.agentSelector = agentSelector;
        this.skillSelector = skillSelector;
        this.toolSelector = toolSelector;
        this.modelRouter = modelRouter;
        this.logger = logger;
    }

    /// <inheritdoc />
    public ExecutionPlan Plan(
        string input,
        IAgentRegistry registry,
        IReadOnlyCollection<ISkill>? skills = null,
        IReadOnlyCollection<ITool>? tools = null)
    {
        var intent = intentAnalyzer.Analyze(input);
        var analysis = taskAnalyzer.Analyze(input, intent);
        var workflow = workflowPlanner.Plan(input, intent, analysis);
        var routing = agentSelector.SelectAgent(intent, registry);
        routing = skillSelector.SelectSkill(routing, skills ?? []);
        routing = toolSelector.SelectTool(routing, tools ?? []);
        routing = modelRouter.Route(routing, intent);

        workflow.Plan.Routing = routing;
        LogPlan(workflow.Plan);

        return workflow.Plan;
    }

    private void LogPlan(ExecutionPlan plan)
    {
        if (logger is null)
        {
            return;
        }

        _ = logger.LogAsync(new FrameworkLogEntry
        {
            Level = FrameworkLogLevel.Information,
            Message = "Brain created execution plan.",
            CorrelationId = plan.PlanId
        });
    }
}
