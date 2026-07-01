namespace Jarvis.Core.Agents.Coding.FileGeneration;

/// <summary>
/// Represents one generated project file.
/// </summary>
public sealed class GeneratedFile
{
    /// <summary>
    /// Gets or sets the relative path.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the written full path.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;
}
