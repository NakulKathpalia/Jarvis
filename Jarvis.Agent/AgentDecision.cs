namespace Jarvis.Agent;

public sealed class AgentDecision
{
    public AgentDecision(
        AgentAction action,
        string target,
        string reason,
        bool requiresConfirmation = false)
    {
        Action = action;
        Target = target;
        Reason = reason;
        RequiresConfirmation = requiresConfirmation;
    }

    public AgentAction Action { get; }

    public string Target { get; }

    public string Reason { get; }

    public bool RequiresConfirmation { get; }

    public static AgentDecision None(string reason) =>
        new(AgentAction.None, string.Empty, reason);
}
