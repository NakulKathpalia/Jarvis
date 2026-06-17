using Jarvis.Services;

namespace Jarvis.Commands;

public sealed class AppCommandService : ICommand
{
    private readonly PcCommandService _pcCommandService;

    public AppCommandService(PcCommandService pcCommandService)
    {
        _pcCommandService = pcCommandService;
    }

    public string Name => "app";
    public string Description => "Run known PC control commands with safety checks.";
    public string Usage => "/app <command> or /app confirm <confirmation-id>";

    public async Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }

        var parts = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var result = parts.Length == 2 && parts[0].Equals("confirm", StringComparison.OrdinalIgnoreCase)
            ? await _pcCommandService.ConfirmAsync(parts[1].Trim(), cancellationToken)
            : await _pcCommandService.ExecuteAsync(arguments.Trim(), cancellationToken: cancellationToken);

        Console.WriteLine(result.Message);
        if (result.RequiresConfirmation)
        {
            Console.WriteLine($"Confirm with: /app confirm {result.ConfirmationId}");
        }
    }
}
