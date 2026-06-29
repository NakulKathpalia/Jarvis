namespace Jarvis.Core.Agents.Coding.AI;

/// <summary>
/// Represents a local coding model request.
/// </summary>
public sealed class CodingModelRequest
{
    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the prompt text.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets file paths included in prompt context.
    /// </summary>
    public IList<string> ContextFilePaths { get; } = [];

    /// <summary>
    /// Gets symbols included in prompt context.
    /// </summary>
    public IList<string> ContextSymbols { get; } = [];
}
