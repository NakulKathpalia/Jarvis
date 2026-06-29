namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a file selected as factual coding context.
/// </summary>
public sealed class RelevantFile
{
    /// <summary>
    /// Gets or sets the repository-relative file path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected language.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relevance score.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets the first source line included.
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// Gets or sets the last source line included.
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// Gets or sets source snippet text.
    /// </summary>
    public string SourceSnippet { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets import statements found in the file.
    /// </summary>
    public List<string> ImportStatements { get; set; } = [];
}
