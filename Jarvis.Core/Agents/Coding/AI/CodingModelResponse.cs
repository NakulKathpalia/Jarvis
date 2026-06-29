namespace Jarvis.Core.Agents.Coding.AI;

/// <summary>
/// Represents a local coding model response.
/// </summary>
public sealed class CodingModelResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the request succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets response text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the original prompt size.
    /// </summary>
    public int OriginalPromptSize { get; set; }

    /// <summary>
    /// Gets or sets the compressed prompt size.
    /// </summary>
    public int CompressedPromptSize { get; set; }

    /// <summary>
    /// Gets validation and execution warnings.
    /// </summary>
    public IList<string> Warnings { get; } = [];

    /// <summary>
    /// Gets or sets the finish reason.
    /// </summary>
    public string FinishReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw provider response.
    /// </summary>
    public string RawProviderResponse { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the response came from cache.
    /// </summary>
    public bool FromCache { get; set; }

    /// <summary>
    /// Gets or sets provider health status observed before execution.
    /// </summary>
    public string ProviderHealthStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets model health status observed before execution.
    /// </summary>
    public string ModelHealthStatus { get; set; } = string.Empty;
}
