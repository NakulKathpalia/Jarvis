namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

/// <summary>
/// Defines character and selection limits for AI-ready context.
/// </summary>
public sealed class ContextBudget
{
    /// <summary>
    /// Gets or sets the maximum character count.
    /// </summary>
    public int MaxCharacters { get; set; } = 24000;

    /// <summary>
    /// Gets or sets the maximum number of files.
    /// </summary>
    public int MaxFiles { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum number of symbols.
    /// </summary>
    public int MaxSymbols { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum snippet lines per file.
    /// </summary>
    public int MaxSnippetLines { get; set; } = 40;
}
