namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Brain.Models;

/// <summary>
/// Represents a generic workflow created from an execution plan.
/// </summary>
public sealed class Workflow
{
    /// <summary>
    /// Gets or sets the workflow identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the workflow execution plan.
    /// </summary>
    public ExecutionPlan ExecutionPlan { get; set; } = new();

    /// <summary>
    /// Gets or sets the workflow steps.
    /// </summary>
    public List<WorkflowStep> Steps { get; set; } = [];

    /// <summary>
    /// Gets or sets non-nested workflow branches.
    /// </summary>
    public List<WorkflowBranch> Branches { get; set; } = [];

    /// <summary>
    /// Gets or sets workflow metadata.
    /// </summary>
    public WorkflowMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets workflow options.
    /// </summary>
    public WorkflowOptions Options { get; set; } = new();

    /// <summary>
    /// Gets or sets the current workflow state.
    /// </summary>
    public WorkflowState State { get; set; } = WorkflowState.Created;

    /// <summary>
    /// Creates a workflow from a Brain execution plan.
    /// </summary>
    /// <param name="plan">The execution plan.</param>
    /// <returns>A workflow containing mapped plan steps.</returns>
    public static Workflow FromExecutionPlan(ExecutionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new Workflow
        {
            ExecutionPlan = plan,
            Metadata = new WorkflowMetadata
            {
                Name = plan.Intent.Intent,
                Description = plan.Input
            },
            Steps = plan.Steps
                .OrderBy(step => step.Order)
                .Select(step => new WorkflowStep
                {
                    Id = step.StepId,
                    Name = step.StepType,
                    Order = step.Order,
                    TaskType = step.StepType,
                    Input = step.Input
                })
                .ToList()
        };
    }
}
