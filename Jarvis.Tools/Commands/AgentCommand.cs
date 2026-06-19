using Jarvis.Agent;

namespace Jarvis.Commands;

public sealed class AgentCommand : ICommand
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
        if (string.IsNullOrWhiteSpace(arguments))
        {
            Console.WriteLine($"Usage: {Usage}");
            return Task.CompletedTask;
        }

        var decision = _planner.Plan(arguments);

        // Dry run only: print the decision and never dispatch the action.
        Console.WriteLine($"Action: {ToDisplayAction(decision.Action)}");
        Console.WriteLine($"Payload: {arguments.Trim()}");
        Console.WriteLine($"PlannedAction: {decision.Action}");
        Console.WriteLine($"Target: {decision.Target}");
        Console.WriteLine($"Reason: {decision.Reason}");
        Console.WriteLine($"RequiresConfirmation: {decision.RequiresConfirmation}");

        return Task.CompletedTask;
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
