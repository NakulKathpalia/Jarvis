namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Describes a simple condition evaluated against a previous workflow step result.
/// </summary>
public sealed class WorkflowCondition
{
    /// <summary>
    /// Gets or sets the step identifier whose result should be evaluated.
    /// </summary>
    public string SourceStepId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expected success value, when success should be checked.
    /// </summary>
    public bool? ExpectedSucceeded { get; set; }

    /// <summary>
    /// Gets or sets the exact output expected from the source step.
    /// </summary>
    public string OutputEquals { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets text that must appear in the source step output.
    /// </summary>
    public string OutputContains { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets text that must appear in the source step error message.
    /// </summary>
    public string ErrorContains { get; set; } = string.Empty;
}
