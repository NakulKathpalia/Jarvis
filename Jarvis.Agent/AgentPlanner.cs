namespace Jarvis.Agent;

public sealed class AgentPlanner
{
    // This first planner is deliberately rule-based. It does not call an LLM
    // and is not wired into the Jarvis app yet.
    public AgentDecision Plan(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return AgentDecision.None("No input was provided.");
        }

        var trimmed = input.Trim();
        var normalized = trimmed.ToLowerInvariant();

        if (normalized.StartsWith("remember ", StringComparison.OrdinalIgnoreCase))
        {
            return new AgentDecision(
                AgentAction.Remember,
                trimmed["remember ".Length..].Trim(),
                "Input starts with a memory instruction.");
        }

        if (normalized.StartsWith("search files for ", StringComparison.OrdinalIgnoreCase))
        {
            return new AgentDecision(
                AgentAction.SearchFiles,
                trimmed["search files for ".Length..].Trim(),
                "Input requests a local file search.");
        }

        if (normalized.StartsWith("open website ", StringComparison.OrdinalIgnoreCase))
        {
            return new AgentDecision(
                AgentAction.OpenWebsite,
                trimmed["open website ".Length..].Trim(),
                "Input requests opening a website.");
        }

        if (normalized.StartsWith("open app ", StringComparison.OrdinalIgnoreCase))
        {
            return new AgentDecision(
                AgentAction.OpenApp,
                trimmed["open app ".Length..].Trim(),
                "Input requests opening an application.");
        }

        if (LooksLikeSensitiveLocalCommand(normalized))
        {
            return new AgentDecision(
                AgentAction.RunLocalCommand,
                trimmed,
                "Input appears to request a sensitive local command.",
                requiresConfirmation: true);
        }

        return new AgentDecision(
            AgentAction.Chat,
            trimmed,
            "No tool-specific rule matched, so treat the input as chat.");
    }

    // Sample helper for future planner expansion. Keep safety-sensitive actions
    // explicit and conservative until the app has a stronger policy layer.
    public bool RequiresConfirmation(AgentAction action) =>
        action is AgentAction.RunLocalCommand;

    // Sample catalog for UI/debug views once the agent is integrated.
    public IReadOnlyCollection<AgentAction> GetSupportedActions() =>
    [
        AgentAction.Chat,
        AgentAction.SearchFiles,
        AgentAction.OpenApp,
        AgentAction.OpenWebsite,
        AgentAction.Remember,
        AgentAction.RunLocalCommand
    ];

    private static bool LooksLikeSensitiveLocalCommand(string normalizedInput)
    {
        return normalizedInput.Contains("shutdown", StringComparison.OrdinalIgnoreCase)
            || normalizedInput.Contains("restart", StringComparison.OrdinalIgnoreCase)
            || normalizedInput.Contains("sleep", StringComparison.OrdinalIgnoreCase);
    }
}
