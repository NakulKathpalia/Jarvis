namespace Jarvis.Commands;

public sealed class HelpCommand : ICommand
{
    private readonly CommandManager _commandManager;

    public HelpCommand(CommandManager commandManager)
    {
        _commandManager = commandManager;
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
        Console.ResetColor();
        return Task.CompletedTask;
    }
}
