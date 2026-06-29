namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a language-independent source code symbol.
/// </summary>
public class CodeSymbol
{
    /// <summary>
    /// Gets or sets the symbol identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the symbol name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the symbol kind.
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source language.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source file path.
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the one-based source line.
    /// </summary>
    public int Line { get; set; }

    /// <summary>
    /// Gets or sets the parent symbol identifier.
    /// </summary>
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets child symbols.
    /// </summary>
    public List<CodeSymbol> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the declared accessibility.
    /// </summary>
    public string Accessibility { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets symbol modifiers.
    /// </summary>
    public List<string> Modifiers { get; set; } = [];

    /// <summary>
    /// Gets or sets symbol attributes.
    /// </summary>
    public List<string> Attributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the project containing this symbol.
    /// </summary>
    public string Project { get; set; } = string.Empty;
}
