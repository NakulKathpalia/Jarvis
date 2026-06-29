namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a namespace symbol.
/// </summary>
public sealed class NamespaceSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceSymbol"/> class.
    /// </summary>
    public NamespaceSymbol()
    {
        Kind = "Namespace";
    }
}
