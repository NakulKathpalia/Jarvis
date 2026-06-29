namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents framework-only workflow diagnostics.
/// </summary>
public sealed class WorkflowDiagnostics
{
    /// <summary>
    /// Gets or sets the workflow identifier.
    /// </summary>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the workflow structure is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets diagnostic messages.
    /// </summary>
    public List<string> Messages { get; set; } = [];

    /// <summary>
    /// Gets or sets the workflow progress snapshot.
    /// </summary>
    public WorkflowProgress Progress { get; set; } = new();

    /// <summary>
    /// Gets or sets the workflow metrics snapshot.
    /// </summary>
    public WorkflowMetrics Metrics { get; set; } = new();
}
