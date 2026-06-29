namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Produces framework-only workflow diagnostics without executing workflows.
/// </summary>
public sealed class WorkflowInspector
{
    private readonly WorkflowValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowInspector"/> class.
    /// </summary>
    /// <param name="validator">The workflow validator.</param>
    public WorkflowInspector(WorkflowValidator? validator = null)
    {
        this.validator = validator ?? new WorkflowValidator();
    }

    /// <summary>
    /// Inspects a workflow and returns diagnostics.
    /// </summary>
    /// <param name="workflow">The workflow to inspect.</param>
    /// <param name="progress">The optional progress snapshot.</param>
    /// <param name="metrics">The optional metrics snapshot.</param>
    /// <returns>The workflow diagnostics.</returns>
    public WorkflowDiagnostics Inspect(
        Workflow workflow,
        WorkflowProgress? progress = null,
        WorkflowMetrics? metrics = null)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        var diagnostics = new WorkflowDiagnostics
        {
            WorkflowId = workflow.Id,
            Progress = progress ?? new WorkflowProgress(),
            Metrics = metrics ?? new WorkflowMetrics()
        };

        try
        {
            validator.Validate(workflow);
            diagnostics.IsValid = true;
            diagnostics.Messages.Add("Workflow is valid.");
        }
        catch (Exception ex)
        {
            diagnostics.IsValid = false;
            diagnostics.Messages.Add(ex.Message);
        }

        return diagnostics;
    }
}
