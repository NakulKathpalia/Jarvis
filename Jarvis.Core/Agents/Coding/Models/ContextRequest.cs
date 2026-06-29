namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a request for factual coding context.
/// </summary>
public sealed class ContextRequest
{
    /// <summary>
    /// Gets or sets the coding request text.
    /// </summary>
    public string RequestText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the context window settings.
    /// </summary>
    public ContextWindow Window { get; set; } = new();
}
