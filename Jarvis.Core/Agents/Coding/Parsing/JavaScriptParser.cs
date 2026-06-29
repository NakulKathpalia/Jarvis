namespace Jarvis.Core.Agents.Coding.Parsing;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Provides a compile-safe JavaScript parser placeholder.
/// </summary>
public sealed class JavaScriptParser : LanguageParserBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaScriptParser"/> class.
    /// </summary>
    public JavaScriptParser()
        : base("JavaScript")
    {
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeSymbol> Parse(RepositoryFile file, string sourceText)
    {
        return [];
    }
}
