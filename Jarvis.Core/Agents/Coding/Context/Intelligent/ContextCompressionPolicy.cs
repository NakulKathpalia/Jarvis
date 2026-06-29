namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

/// <summary>
/// Defines context compression behavior.
/// </summary>
public enum ContextCompressionPolicy
{
    /// <summary>
    /// Do not compress context.
    /// </summary>
    None,

    /// <summary>
    /// Trim only when context exceeds the configured budget.
    /// </summary>
    BudgetAware,

    /// <summary>
    /// Aggressively reduce snippets and duplicate imports.
    /// </summary>
    Aggressive
}
