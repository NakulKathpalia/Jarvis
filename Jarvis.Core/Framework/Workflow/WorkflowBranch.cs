namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a non-nested IF/ELSE/END branch in a workflow.
/// </summary>
public sealed class WorkflowBranch
{
    /// <summary>
    /// Gets or sets the branch identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the condition that selects the IF or ELSE path.
    /// </summary>
    public WorkflowCondition Condition { get; set; } = new();

    /// <summary>
    /// Gets or sets step identifiers that belong to the IF path.
    /// </summary>
    public List<string> IfStepIds { get; set; } = [];

    /// <summary>
    /// Gets or sets step identifiers that belong to the ELSE path.
    /// </summary>
    public List<string> ElseStepIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional step identifier where the branch joins.
    /// </summary>
    public string EndStepId { get; set; } = string.Empty;
}
