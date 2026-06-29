namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents indexed repository file information.
/// </summary>
public sealed class FileIndex
{
    /// <summary>
    /// Gets or sets indexed files.
    /// </summary>
    public List<RepositoryFile> Files { get; set; } = [];
}
