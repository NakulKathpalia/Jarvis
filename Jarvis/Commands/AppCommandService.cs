using Jarvis.Services;

namespace Jarvis.Commands;

public sealed class AppCommandService : ICommand
{
    private readonly InstalledAppService _appService;

    public AppCommandService(InstalledAppService appService)
    {
        _appService = appService;
    }

    public string Name => "app";
    public string Description => "Open an app, file, folder, or URL.";
    public string Usage => "/app open <target>";

    public async Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        var parts = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !parts[0].Equals("open", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Usage: {Usage}");
            return;
        }

        var opened = await _appService.OpenAsync(parts[1].Trim());
        Console.WriteLine(opened ? "Opened." : "Could not open target.");
    }
}
