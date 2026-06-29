namespace Jarvis.Core.Agents.Coding.Parsing;

/// <summary>
/// Registers and resolves language parsers.
/// </summary>
public sealed class LanguageParserRegistry
{
    private readonly Dictionary<string, ILanguageParser> parsers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a language parser.
    /// </summary>
    /// <param name="parser">The parser to register.</param>
    public void Register(ILanguageParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);
        parsers[parser.Language] = parser;
    }

    /// <summary>
    /// Attempts to resolve a parser for a language.
    /// </summary>
    /// <param name="language">The source language.</param>
    /// <param name="parser">The parser when found.</param>
    /// <returns><c>true</c> when a parser is registered.</returns>
    public bool TryGetParser(string language, out ILanguageParser? parser)
    {
        return parsers.TryGetValue(language, out parser);
    }
}
