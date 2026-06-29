namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a single executable workflow step.
/// </summary>
public sealed class WorkflowStep
{
    /// <summary>
    /// Gets or sets the workflow step identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequential order hint.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the framework task type to execute.
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the framework task input.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifiers of steps that must complete first.
    /// </summary>
    public List<string> Dependencies { get; set; } = [];

    /// <summary>
    /// Gets or sets optional framework task parameters.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional retry policy for this step.
    /// </summary>
    public IRetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Gets or sets the optional confirmation request that must complete before this step runs.
    /// </summary>
    public ConfirmationRequest? ConfirmationRequest { get; set; }

    /// <summary>
    /// Gets or sets the optional approval request that must complete before this step runs.
    /// </summary>
    public ApprovalRequest? ApprovalRequest { get; set; }
}
