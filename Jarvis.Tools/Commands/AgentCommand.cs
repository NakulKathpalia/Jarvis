using Jarvis.Agent;

namespace Jarvis.Commands;

public sealed class AgentCommand : ICommandWithResult
{
    private readonly AgentPlanner _planner;

    public AgentCommand(AgentPlanner planner)
    {
        _planner = planner;
    }

    public string Name => "agent";
    public string Description => "Preview what the agent would do without executing anything.";
    public string Usage => "/agent <input>";

    public Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        return ExecuteWithResultAsync(arguments, cancellationToken);
    }

    public Task<string> ExecuteWithResultAsync(string arguments, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            var usage = $"Usage: {Usage}";
            Console.WriteLine(usage);
            return Task.FromResult(usage);
        }

        var decision = _planner.Plan(arguments);
        var result = FormatDecision(decision);

        // Dry run only: print the decision and never dispatch the action.
        Console.WriteLine(result);

        return Task.FromResult(result);
    }

    private static string FormatDecision(AgentDecision decision)
    {
        return string.Join(Environment.NewLine,
        [
            $"Action: {ToDisplayAction(decision.Action)}",
            $"PlannedAction: {decision.Action}",
            $"Target: {decision.Target}",
            $"Reason: {decision.Reason}",
            $"RequiresConfirmation: {decision.RequiresConfirmation}"
        ]);
    }

    private static string ToDisplayAction(AgentAction action)
    {
        return action switch
        {
            AgentAction.None => "None",
            AgentAction.Chat => "Chat",
            _ => "Tool"
        };
    }
}
