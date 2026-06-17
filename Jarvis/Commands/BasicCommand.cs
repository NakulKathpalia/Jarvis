namespace Jarvis.Commands;

public sealed class BasicCommand : ICommand
{
    private readonly CommandManager _commandManager;
    private readonly Func<bool> _requestExit;

    public BasicCommand(CommandManager commandManager, Func<bool> requestExit)
    {
        _commandManager = commandManager;
        _requestExit = requestExit;
    }

    public string Name => "help";
    public string Description => "Show all commands.";
    public string Usage => "/help";

    public Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Available commands:");
        foreach (var command in _commandManager.Commands.OrderBy(command => command.Name))
        {
            Console.WriteLine($"  {command.Usage} - {command.Description}");
        }
        Console.WriteLine("  /exit - Close Jarvis");
        Console.ResetColor();

        _ = _requestExit;
        return Task.CompletedTask;
    }
}
