namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a constructor symbol.
/// </summary>
public sealed class ConstructorSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConstructorSymbol"/> class.
    /// </summary>
    public ConstructorSymbol()
    {
        Kind = "Constructor";
    }
}
