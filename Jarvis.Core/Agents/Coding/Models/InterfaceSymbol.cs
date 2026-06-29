namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents an interface symbol.
/// </summary>
public sealed class InterfaceSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceSymbol"/> class.
    /// </summary>
    public InterfaceSymbol()
    {
        Kind = "Interface";
    }
}
