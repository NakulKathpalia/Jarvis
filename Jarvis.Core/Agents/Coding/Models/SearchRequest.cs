namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a repository search request.
/// </summary>
public sealed class SearchRequest
{
    /// <summary>
    /// Gets or sets a file name query.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an extension query.
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a folder query.
    /// </summary>
    public string Folder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a project query.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a configuration query.
    /// </summary>
    public string Configuration { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a language query.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets search options.
    /// </summary>
    public SearchOptions Options { get; set; } = new();
}
