namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;

/// <summary>
/// Evaluates workflow branch conditions against completed step results.
/// </summary>
public sealed class ConditionEvaluator
{
    /// <summary>
    /// Evaluates a workflow condition.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="stepResults">Completed step results by step identifier.</param>
    /// <returns><c>true</c> when the condition matches the source result.</returns>
    public bool Evaluate(WorkflowCondition condition, IReadOnlyDictionary<string, TaskResult> stepResults)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(stepResults);

        if (!stepResults.TryGetValue(condition.SourceStepId, out var result))
        {
            return false;
        }

        if (condition.ExpectedSucceeded.HasValue && result.Succeeded != condition.ExpectedSucceeded.Value)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(condition.OutputEquals) &&
            !string.Equals(result.Output, condition.OutputEquals, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(condition.OutputContains) &&
            !result.Output.Contains(condition.OutputContains, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(condition.ErrorContains) &&
            !result.ErrorMessage.Contains(condition.ErrorContains, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }
}
