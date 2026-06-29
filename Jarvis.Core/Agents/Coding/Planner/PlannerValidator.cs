namespace Jarvis.Core.Agents.Coding.Planner;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Validates read-only coding planning requests and results.
/// </summary>
public sealed class PlannerValidator
{
    /// <summary>
    /// Validates a planning request.
    /// </summary>
    public IReadOnlyList<string> Validate(PlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = new List<string>();
        if (string.IsNullOrWhiteSpace(request.RequestText))
        {
            messages.Add("Coding request text is required.");
        }

        if (request.ContextPackage.Statistics.RelevantFileCount == 0)
        {
            messages.Add("No relevant files were selected.");
        }

        return messages;
    }
}
