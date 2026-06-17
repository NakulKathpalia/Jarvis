using Jarvis.Memory;

namespace Jarvis.Services;

public sealed class VoiceCommandService
{
    private static readonly IReadOnlyCollection<VoiceCommandCatalogItem> CommandCatalog =
    [
        new(
            "Memory",
            "Save a local memory item.",
            ["remember that I prefer local AI", "save memory project name is Jarvis"],
            false),
        new(
            "Files",
            "Search or rebuild the local file index.",
            ["search files for Program.cs", "index files"],
            false),
        new(
            "Model",
            "Switch the local Ollama model.",
            ["switch model to llama3.2:3b", "set model to mistral"],
            false),
        new(
            "Apps",
            "Open a local app, file, folder, or URL after confirmation.",
            ["open notepad", "launch chrome"],
            true),
        new(
            "Help",
            "Show available local voice commands.",
            ["voice help", "list voice commands"],
            false)
    ];

    private readonly MemoryService _memoryService;
    private readonly FileIndexService _fileIndexService;
    private readonly InstalledAppService _installedAppService;
    private readonly SettingsService _settingsService;

    public VoiceCommandService(
        MemoryService memoryService,
        FileIndexService fileIndexService,
        InstalledAppService installedAppService,
        SettingsService settingsService)
    {
        _memoryService = memoryService;
        _fileIndexService = fileIndexService;
        _installedAppService = installedAppService;
        _settingsService = settingsService;
    }

    public async Task<VoiceCommandResult> TryExecuteAsync(
        string transcript,
        bool confirmed = false,
        CancellationToken cancellationToken = default)
    {
        var input = transcript.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return VoiceCommandResult.NoMatch();
        }

        var normalized = Normalize(input);

        if (IsHelpCommand(normalized))
        {
            return VoiceCommandResult.Done(
                "voice.help",
                BuildHelpMessage(),
                CommandCatalog.SelectMany(command => command.Examples).ToList());
        }

        if (TryStripPrefix(normalized, ["remember that ", "remember ", "save memory ", "memorize "], out var memoryText))
        {
            await _memoryService.AddAsync(memoryText, "Voice", cancellationToken);
            return VoiceCommandResult.Done("memory.add", $"Saved to memory: {memoryText}");
        }

        if (TryStripPrefix(normalized, ["search files for ", "find file ", "find files ", "search file "], out var fileQuery))
        {
            if (_fileIndexService.IndexedFiles.Count == 0)
            {
                await _fileIndexService.RebuildAsync(cancellationToken);
            }

            var results = _fileIndexService.Search(fileQuery).ToList();
            var message = results.Count == 0
                ? $"No local files matched: {fileQuery}"
                : $"Found {results.Count} file match(es):\n{string.Join("\n", results.Take(5))}";

            return VoiceCommandResult.Done("files.search", message, results);
        }

        if (TryStripPrefix(normalized, ["index files", "reindex files", "scan files"], out _))
        {
            var count = await _fileIndexService.RebuildAsync(cancellationToken);
            return VoiceCommandResult.Done("files.index", $"Indexed {count} files.");
        }

        if (TryStripPrefix(normalized, ["change model to ", "switch model to ", "set model to "], out var modelName))
        {
            _settingsService.Current.Model = modelName;
            await _settingsService.SaveAsync(cancellationToken);
            return VoiceCommandResult.Done("settings.model", $"Model changed to {modelName}.");
        }

        if (TryStripPrefix(normalized, ["open ", "launch ", "start "], out var target))
        {
            if (!confirmed)
            {
                return VoiceCommandResult.NeedsConfirmation(
                    "app.open",
                    $"Opening '{target}' will launch an app, file, folder, or URL.",
                    target);
            }

            var opened = await _installedAppService.OpenAsync(target);
            return opened
                ? VoiceCommandResult.Done("app.open", $"Opened {target}.")
                : VoiceCommandResult.Done("app.open", $"Could not open {target}.");
        }

        return VoiceCommandResult.NoMatch();
    }

    public IReadOnlyCollection<VoiceCommandCatalogItem> GetCatalog() => CommandCatalog;

    private static string Normalize(string value)
    {
        return value.Trim().TrimEnd('.', '!', '?').ToLowerInvariant();
    }

    private static bool TryStripPrefix(string input, IReadOnlyCollection<string> prefixes, out string value)
    {
        foreach (var prefix in prefixes)
        {
            if (input.Equals(prefix.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                value = string.Empty;
                return true;
            }

            if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = input[prefix.Length..].Trim();
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool IsHelpCommand(string input)
    {
        return input is "voice help"
            or "voice commands"
            or "list voice commands"
            or "show voice commands"
            or "what voice commands can you do"
            or "what commands can you do";
    }

    private static string BuildHelpMessage()
    {
        var lines = CommandCatalog
            .Select(command => $"{command.Category}: {string.Join(" | ", command.Examples)}");

        return "Available local voice commands:\n" + string.Join("\n", lines);
    }
}

public sealed record VoiceCommandCatalogItem(
    string Category,
    string Description,
    IReadOnlyCollection<string> Examples,
    bool RequiresConfirmation);

public sealed record VoiceCommandResult(
    bool Handled,
    bool RequiresConfirmation,
    string Command,
    string Message,
    string? ConfirmationValue = null,
    IReadOnlyCollection<string>? Results = null)
{
    public static VoiceCommandResult NoMatch() =>
        new(false, false, string.Empty, "No local voice command matched.");

    public static VoiceCommandResult Done(
        string command,
        string message,
        IReadOnlyCollection<string>? results = null) =>
        new(true, false, command, message, Results: results);

    public static VoiceCommandResult NeedsConfirmation(
        string command,
        string message,
        string confirmationValue) =>
        new(false, true, command, message, confirmationValue);
}
