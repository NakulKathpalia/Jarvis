namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;

/// <summary>
/// Defines conversion from task analysis to execution plan.
/// </summary>
public interface IWorkflowPlanner
{
    /// <summary>
    /// Creates an execution plan from analysis.
    /// </summary>
    /// <param name="input">The original input.</param>
    /// <param name="intent">The detected intent.</param>
    /// <param name="analysis">The task analysis.</param>
    /// <returns>The workflow result.</returns>
    WorkflowResult Plan(string input, IntentResult intent, TaskAnalysis analysis);
}
