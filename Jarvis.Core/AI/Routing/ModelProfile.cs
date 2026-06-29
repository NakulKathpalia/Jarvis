namespace Jarvis.Core.AI.Routing;

/// <summary>
/// Describes the provider and model selected for a request.
/// </summary>
public sealed class ModelProfile
{
    /// <summary>
    /// Gets or sets the request purpose.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    public string ProviderName { get; set; } = "Ollama";

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;
}
