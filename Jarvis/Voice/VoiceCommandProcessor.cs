using Jarvis.Models;
using Jarvis.Services;

namespace Jarvis.Voice;

public sealed class VoiceCommandProcessor
{
    private readonly PcCommandParser _parser;
    private readonly PcCommandService _pcCommandService;

    public VoiceCommandProcessor(
        PcCommandParser parser,
        PcCommandService pcCommandService)
    {
        _parser = parser;
        _pcCommandService = pcCommandService;
    }

    public PcCommand Parse(string transcript) => _parser.Parse(transcript);

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

        var parsed = _parser.Parse(input);
        if (parsed.Action == PcControlAction.Unknown)
        {
            return VoiceCommandResult.NoMatch();
        }

        var result = await _pcCommandService.ExecuteAsync(input, confirmed, cancellationToken);
        return ToVoiceResult(result);
    }

    public async Task<VoiceCommandResult> ConfirmAsync(
        string confirmationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _pcCommandService.ConfirmAsync(confirmationId, cancellationToken);
        return ToVoiceResult(result);
    }

    public IReadOnlyCollection<VoiceCommandCatalogItem> GetCatalog() =>
        _pcCommandService.Catalog
            .Select(command => new VoiceCommandCatalogItem(
                "PC Control",
                command.Description,
                command.Examples,
                command.SafetyLevel == CommandSafetyLevel.ConfirmationRequired))
            .ToList();

    private static VoiceCommandResult ToVoiceResult(PcCommandExecutionResult result)
    {
        if (result.RequiresConfirmation)
        {
            return VoiceCommandResult.NeedsConfirmation(
                result.Command,
                result.Message,
                result.ConfirmationId ?? result.ConfirmationToken ?? string.Empty);
        }

        if (string.IsNullOrWhiteSpace(result.Command))
        {
            return VoiceCommandResult.NoMatch();
        }

        return result.Handled
            ? VoiceCommandResult.Done(result.Command, result.Message)
            : new VoiceCommandResult(false, false, result.Command, result.Message);
    }
}
