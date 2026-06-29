namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents factual code intelligence for a repository.
/// </summary>
public sealed class RepositoryKnowledge
{
    /// <summary>
    /// Gets or sets the total symbol count.
    /// </summary>
    public int SymbolCount { get; set; }

    /// <summary>
    /// Gets or sets the class count.
    /// </summary>
    public int ClassCount { get; set; }

    /// <summary>
    /// Gets or sets the method count.
    /// </summary>
    public int MethodCount { get; set; }

    /// <summary>
    /// Gets or sets the interface count.
    /// </summary>
    public int InterfaceCount { get; set; }

    /// <summary>
    /// Gets or sets the property count.
    /// </summary>
    public int PropertyCount { get; set; }

    /// <summary>
    /// Gets or sets the field count.
    /// </summary>
    public int FieldCount { get; set; }

    /// <summary>
    /// Gets or sets discovered namespaces.
    /// </summary>
    public List<string> Namespaces { get; set; } = [];

    /// <summary>
    /// Gets or sets largest classes by child member count.
    /// </summary>
    public List<string> LargestClasses { get; set; } = [];

    /// <summary>
    /// Gets or sets largest files by symbol count.
    /// </summary>
    public List<string> LargestFiles { get; set; } = [];

    /// <summary>
    /// Gets or sets the average method count per class.
    /// </summary>
    public double AverageMethodsPerClass { get; set; }
}
