namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Represents a generated runnable file.
/// </summary>
public sealed class RunnableFile
{
    /// <summary>
    /// Gets or sets the relative path.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute path.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
