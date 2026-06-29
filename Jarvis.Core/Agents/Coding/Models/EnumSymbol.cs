namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents an enum symbol.
/// </summary>
public sealed class EnumSymbol : CodeSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumSymbol"/> class.
    /// </summary>
    public EnumSymbol()
    {
        Kind = "Enum";
    }
}
