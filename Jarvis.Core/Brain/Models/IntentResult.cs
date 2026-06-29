namespace Jarvis.Core.Brain.Models;

/// <summary>
/// Represents the rule-based intent detected for a request.
/// </summary>
public sealed class IntentResult
{
    /// <summary>
    /// Gets or sets the detected intent name.
    /// </summary>
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generic task type associated with the intent.
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score from zero to one.
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Gets or sets the reason for the intent decision.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
