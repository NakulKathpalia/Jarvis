using Jarvis.Models;
using Jarvis.Voice;

namespace Jarvis.Services;

public sealed class VoiceCommandService
{
    private readonly VoiceCommandProcessor _processor;

    public VoiceCommandService(
        VoiceCommandProcessor processor)
    {
        _processor = processor;
    }

    public async Task<VoiceCommandResult> TryExecuteAsync(
        string transcript,
        bool confirmed = false,
        CancellationToken cancellationToken = default)
    {
        return await _processor.TryExecuteAsync(transcript, confirmed, cancellationToken);
    }

    public IReadOnlyCollection<VoiceCommandCatalogItem> GetCatalog() => _processor.GetCatalog();
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
