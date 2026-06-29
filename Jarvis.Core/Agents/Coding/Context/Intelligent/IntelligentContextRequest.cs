namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Represents a request for AI-ready coding context.
/// </summary>
public sealed class IntelligentContextRequest
{
    /// <summary>
    /// Gets or sets the user request.
    /// </summary>
    public string UserRequest { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets repository context.
    /// </summary>
    public RepositoryContext RepositoryContext { get; set; } = new();

    /// <summary>
    /// Gets or sets context selection strategy.
    /// </summary>
    public ContextSelectionStrategy Strategy { get; set; } = ContextSelectionStrategy.Balanced;

    /// <summary>
    /// Gets or sets compression policy.
    /// </summary>
    public ContextCompressionPolicy CompressionPolicy { get; set; } = ContextCompressionPolicy.BudgetAware;

    /// <summary>
    /// Gets or sets context budget.
    /// </summary>
    public ContextBudget Budget { get; set; } = new();
}
