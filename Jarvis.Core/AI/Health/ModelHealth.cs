namespace Jarvis.Core.AI.Health;

/// <summary>
/// Represents selected model health.
/// </summary>
public sealed class ModelHealth
{
    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the model is available.
    /// </summary>
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets model status text.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the failure reason.
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;
}
