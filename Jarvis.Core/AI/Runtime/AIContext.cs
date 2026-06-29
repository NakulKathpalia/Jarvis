namespace Jarvis.Core.AI.Runtime;

/// <summary>
/// Contains provider-independent contextual metadata for an AI request.
/// </summary>
public sealed class AIContext
{
    /// <summary>
    /// Gets or sets the request purpose.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository name when available.
    /// </summary>
    public string RepositoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the file paths included in context.
    /// </summary>
    public IList<string> FilePaths { get; } = [];

    /// <summary>
    /// Gets the symbols included in context.
    /// </summary>
    public IList<string> Symbols { get; } = [];

    /// <summary>
    /// Gets additional metadata.
    /// </summary>
    public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
