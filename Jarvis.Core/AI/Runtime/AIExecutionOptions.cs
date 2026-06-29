namespace Jarvis.Core.AI.Runtime;

/// <summary>
/// Defines provider-independent model execution options.
/// </summary>
public sealed class AIExecutionOptions
{
    /// <summary>
    /// Gets or sets the sampling temperature.
    /// </summary>
    public double? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the nucleus sampling value.
    /// </summary>
    public double? TopP { get; set; }

    /// <summary>
    /// Gets or sets the maximum predicted tokens.
    /// </summary>
    public int? NumPredict { get; set; }

    /// <summary>
    /// Gets or sets the context window.
    /// </summary>
    public int? NumContext { get; set; }

    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether streaming is requested.
    /// </summary>
    public bool Streaming { get; set; }

    /// <summary>
    /// Gets or sets the deterministic seed.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Gets the stop tokens.
    /// </summary>
    public IList<string> StopTokens { get; } = [];

    /// <summary>
    /// Gets provider-specific options.
    /// </summary>
    public IDictionary<string, object> ProviderOptions { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
