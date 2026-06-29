namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a property symbol.
/// </summary>
public sealed class PropertySymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertySymbol"/> class.
    /// </summary>
    public PropertySymbol()
    {
        Kind = "Property";
    }
}
