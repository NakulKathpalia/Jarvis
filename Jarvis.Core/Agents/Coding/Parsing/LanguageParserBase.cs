namespace Jarvis.Core.Agents.Coding.Parsing;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Provides shared helpers for language parsers.
/// </summary>
public abstract class LanguageParserBase : ILanguageParser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageParserBase"/> class.
    /// </summary>
    /// <param name="language">The parser language.</param>
    protected LanguageParserBase(string language)
    {
        Language = language;
    }

    /// <inheritdoc />
    public string Language { get; }

    /// <inheritdoc />
    public abstract IReadOnlyList<CodeSymbol> Parse(RepositoryFile file, string sourceText);

    /// <summary>
    /// Creates a stable symbol identifier.
    /// </summary>
    /// <param name="file">The source file.</param>
    /// <param name="kind">The symbol kind.</param>
    /// <param name="name">The symbol name.</param>
    /// <param name="line">The one-based line number.</param>
    /// <returns>The symbol identifier.</returns>
    protected string CreateId(RepositoryFile file, string kind, string name, int line)
    {
        return $"{file.RelativePath}:{line}:{kind}:{name}";
    }
}
