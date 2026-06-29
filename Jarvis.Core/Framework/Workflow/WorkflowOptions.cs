namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Contains generic workflow execution options.
/// </summary>
public sealed class WorkflowOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether execution stops after the first failed step.
    /// </summary>
    public bool StopOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether workflow events should be published.
    /// </summary>
    public bool PublishEvents { get; set; } = true;
}
