namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents indexed repository language information.
/// </summary>
public sealed class LanguageIndex
{
    /// <summary>
    /// Gets or sets file counts by language.
    /// </summary>
    public Dictionary<string, int> FileCounts { get; set; } = [];
}
