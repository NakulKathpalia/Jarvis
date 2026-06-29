namespace Jarvis.Core.Agents.Coding.Parsing;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Defines a parser that extracts language-independent symbols from source files.
/// </summary>
public interface ILanguageParser
{
    /// <summary>
    /// Gets the parser language.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Parses a source file.
    /// </summary>
    /// <param name="file">The repository file.</param>
    /// <param name="sourceText">The source text.</param>
    /// <returns>The symbols discovered in the file.</returns>
    IReadOnlyList<CodeSymbol> Parse(RepositoryFile file, string sourceText);
}
