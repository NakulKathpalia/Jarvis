namespace Jarvis.Core.Brain.Workflow;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;

/// <summary>
/// Converts task analysis into an execution plan.
/// </summary>
public sealed class WorkflowPlanner : IWorkflowPlanner
{
    /// <inheritdoc />
    public WorkflowResult Plan(string input, IntentResult intent, TaskAnalysis analysis)
    {
        return new WorkflowResult
        {
            Succeeded = true,
            Message = "Workflow planned.",
            Plan = new ExecutionPlan
            {
                Input = input ?? string.Empty,
                Intent = intent,
                Analysis = analysis,
                Steps = analysis.Steps
            }
        };
    }
}
