namespace Jarvis.Core.Agents.Coding.Parsing;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Provides a compile-safe TypeScript parser placeholder.
/// </summary>
public sealed class TypeScriptParser : LanguageParserBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeScriptParser"/> class.
    /// </summary>
    public TypeScriptParser()
        : base("TypeScript")
    {
    }

    /// <inheritdoc />
    public override IReadOnlyList<CodeSymbol> Parse(RepositoryFile file, string sourceText)
    {
        return [];
    }
}
