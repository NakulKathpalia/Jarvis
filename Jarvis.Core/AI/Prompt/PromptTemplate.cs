namespace Jarvis.Core.AI.Prompt;

/// <summary>
/// Represents a reusable prompt template.
/// </summary>
public sealed class PromptTemplate
{
    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the system prompt.
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body template.
    /// </summary>
    public string Body { get; set; } = string.Empty;
}
