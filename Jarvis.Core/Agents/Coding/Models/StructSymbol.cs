namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a struct symbol.
/// </summary>
public sealed class StructSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StructSymbol"/> class.
    /// </summary>
    public StructSymbol()
    {
        Kind = "Struct";
    }
}
