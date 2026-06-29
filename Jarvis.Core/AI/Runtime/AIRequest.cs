namespace Jarvis.Core.AI.Runtime;

/// <summary>
/// Represents a provider-independent AI request.
/// </summary>
public sealed class AIRequest
{
    /// <summary>
    /// Gets or sets the user or system purpose for routing.
    /// </summary>
    public string Purpose { get; set; } = "General";

    /// <summary>
    /// Gets or sets the requested provider name.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prompt.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets request context.
    /// </summary>
    public AIContext Context { get; set; } = new();

    /// <summary>
    /// Gets or sets execution options.
    /// </summary>
    public AIExecutionOptions Options { get; set; } = new();
}
