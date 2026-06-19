using Jarvis.Services;

namespace Jarvis.Commands;

public sealed class SettingsCommandService : ICommand
{
    private readonly SettingsService _settingsService;

    public SettingsCommandService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public string Name => "settings";
    public string Description => "Show or update Jarvis settings.";
    public string Usage => "/settings [show|model <name>|root <path>]";

    public async Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        var parts = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var action = parts.Length > 0 ? parts[0].ToLowerInvariant() : "show";
        var value = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        if (action == "show")
        {
            var settings = _settingsService.Current;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Model: {settings.Model}");
            Console.WriteLine($"Ollama: {settings.OllamaBaseUrl}");
            Console.WriteLine($"History: {settings.MaxHistoryMessages}");
            Console.WriteLine($"File root: {settings.FileIndexRoot}");
            Console.ResetColor();
            return;
        }

        if (action == "model" && !string.IsNullOrWhiteSpace(value))
        {
            _settingsService.Current.Model = value;
            await _settingsService.SaveAsync(cancellationToken);
            Console.WriteLine($"Model set to {value}");
            return;
        }

        if (action == "root" && !string.IsNullOrWhiteSpace(value))
        {
            _settingsService.Current.FileIndexRoot = value;
            await _settingsService.SaveAsync(cancellationToken);
            Console.WriteLine($"File index root set to {value}");
            return;
        }

        Console.WriteLine($"Usage: {Usage}");
    }
}
