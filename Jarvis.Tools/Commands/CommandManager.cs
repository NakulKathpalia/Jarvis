namespace Jarvis.Commands;

public sealed class CommandManager
{
    private readonly IReadOnlyCollection<ICommand> _commands;

    public CommandManager(IEnumerable<ICommand> commands)
    {
        _commands = commands as IReadOnlyCollection<ICommand> ?? commands.ToList();
    }

    public IReadOnlyCollection<ICommand> Commands => _commands.ToList();

    public async Task<bool> TryExecuteAsync(string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input) || !input.TrimStart().StartsWith('/'))
        {
            return false;
        }

        var parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var commandName = parts[0].TrimStart('/').ToLowerInvariant();
        var arguments = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        var command = _commands.FirstOrDefault(command =>
            command.Name.TrimStart('/').Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command is null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Unknown command: /{commandName}");
            Console.ResetColor();
            return true;
        }

        await command.ExecuteAsync(arguments, cancellationToken);
        return true;
    }
}
