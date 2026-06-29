namespace Jarvis.Core.Brain.Analysis;

using Jarvis.Core.Brain.Interfaces;
using Jarvis.Core.Brain.Models;

/// <summary>
/// Provides rule-based task analysis.
/// </summary>
public sealed class TaskAnalyzer : ITaskAnalyzer
{
    /// <inheritdoc />
    public TaskAnalysis Analyze(string input, IntentResult intent)
    {
        var segments = SplitInput(input);
        var isMultiStep = segments.Count > 1;

        return new TaskAnalysis
        {
            Complexity = isMultiStep ? "Medium" : "Low",
            IsMultiStep = isMultiStep,
            Steps = segments
                .Select((segment, index) => new ExecutionStep
                {
                    Order = index + 1,
                    Input = segment,
                    StepType = intent.TaskType
                })
                .ToList()
        };
    }

    private static List<string> SplitInput(string input)
    {
        var normalizedInput = input ?? string.Empty;
        var separators = new[] { " then ", " and then " };

        return normalizedInput
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .DefaultIfEmpty(normalizedInput)
            .ToList();
    }
}
