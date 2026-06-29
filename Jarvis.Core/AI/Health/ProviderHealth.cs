namespace Jarvis.Core.AI.Health;

/// <summary>
/// Represents provider health.
/// </summary>
public sealed class ProviderHealth
{
    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the provider is available.
    /// </summary>
    public bool Available { get; set; }

    /// <summary>
    /// Gets or sets provider status text.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the failure reason.
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets available models.
    /// </summary>
    public IList<string> AvailableModels { get; } = [];
}
