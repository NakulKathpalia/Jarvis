namespace Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents factual repository context for the Coding Agent.
/// </summary>
public sealed class RepositoryContext
{
    /// <summary>
    /// Gets or sets the repository index.
    /// </summary>
    public RepositoryIndex Index { get; set; } = new();

    /// <summary>
    /// Gets or sets the repository summary.
    /// </summary>
    public RepositorySummary Summary { get; set; } = new();

    /// <summary>
    /// Gets or sets repository statistics.
    /// </summary>
    public RepositoryStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Gets or sets the symbol index.
    /// </summary>
    public SymbolIndex Symbols { get; set; } = new();

    /// <summary>
    /// Gets or sets factual repository knowledge.
    /// </summary>
    public RepositoryKnowledge Knowledge { get; set; } = new();

    /// <summary>
    /// Gets or sets the request-specific context package.
    /// </summary>
    public ContextPackage ContextPackage { get; set; } = new();

    /// <summary>
    /// Gets or sets the read-only coding planning result.
    /// </summary>
    public PlanningResult PlanningResult { get; set; } = new();

    /// <summary>
    /// Gets or sets repository tree entries.
    /// </summary>
    public List<string> RepositoryTree { get; set; } = [];
}
