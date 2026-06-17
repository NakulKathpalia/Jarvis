using Jarvis.Memory;

namespace Jarvis.Commands;

public sealed class MemoryCommand : ICommand
{
    private readonly MemoryService _memoryService;

    public MemoryCommand(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    public string Name => "memory";
    public string Description => "Manage local JSON memory.";
    public string Usage => "/memory [list|add <text>|clear]";

    public async Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        var parts = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var action = parts.Length > 0 ? parts[0].ToLowerInvariant() : "list";
        var text = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        if (action == "list")
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Memory:");
            foreach (var item in _memoryService.Items)
            {
                Console.WriteLine($"- [{item.Category}] {item.Text}");
            }
            if (_memoryService.Items.Count == 0)
            {
                Console.WriteLine("(empty)");
            }
            Console.ResetColor();
            return;
        }

        if (action == "add" && !string.IsNullOrWhiteSpace(text))
        {
            await _memoryService.AddAsync(text, cancellationToken: cancellationToken);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Memory saved.");
            Console.ResetColor();
            return;
        }

        if (action == "clear")
        {
            await _memoryService.ClearAsync(cancellationToken);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Memory cleared.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine($"Usage: {Usage}");
    }
}
