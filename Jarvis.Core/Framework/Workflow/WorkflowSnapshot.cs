namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a serializable workflow checkpoint snapshot.
/// </summary>
public sealed class WorkflowSnapshot
{
    /// <summary>
    /// Gets or sets the snapshot identifier.
    /// </summary>
    public string SnapshotId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the workflow identifier.
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the workflow state.
    /// </summary>
    public WorkflowState State { get; set; } = WorkflowState.Created;

    /// <summary>
    /// Gets or sets workflow progress at the time of the snapshot.
    /// </summary>
    public WorkflowProgress Progress { get; set; } = new();

    /// <summary>
    /// Gets or sets the completed step identifiers.
    /// </summary>
    public List<string> CompletedStepIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the skipped step identifiers.
    /// </summary>
    public List<string> SkippedStepIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the snapshot creation time in UTC.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
