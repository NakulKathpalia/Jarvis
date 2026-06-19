using Jarvis.Models;

namespace Jarvis.Services;

public sealed class PcCommandService
{
    private readonly PcCommandParser _parser;
    private readonly CommandSafetyService _safetyService;
    private readonly CommandLogService _logService;
    private readonly InteractionLogService? _interactionLogService;
    private readonly IPcControlService _pcControlService;
    private readonly Dictionary<string, PendingPcCommand> _pendingConfirmations = new(StringComparer.OrdinalIgnoreCase);

    public PcCommandService(
        PcCommandParser parser,
        CommandSafetyService safetyService,
        CommandLogService logService,
        IPcControlService pcControlService,
        InteractionLogService? interactionLogService = null)
    {
        _parser = parser;
        _safetyService = safetyService;
        _logService = logService;
        _pcControlService = pcControlService;
        _interactionLogService = interactionLogService;
    }

    public IReadOnlyCollection<PendingPcCommand> PendingConfirmations =>
        _pendingConfirmations.Values
            .Where(pending => pending.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(pending => pending.CreatedAtUtc)
            .ToList();

    public IReadOnlyCollection<PcCommandCatalogItem> Catalog =>
    [
        new("open app", "Open an installed app by name.", CommandSafetyLevel.ConfirmationRequired, ["open chrome", "open notepad"]),
        new("open website", "Open a website in the default browser.", CommandSafetyLevel.ConfirmationRequired, ["open youtube", "open website github.com"]),
        new("open folder", "Open a local folder path.", CommandSafetyLevel.ConfirmationRequired, ["open folder D:\\newfolder"]),
        new("open file", "Open a local file path.", CommandSafetyLevel.ConfirmationRequired, ["open file D:\\notes.txt"]),
        new("browser search", "Search the web in the default browser.", CommandSafetyLevel.Safe, ["search web for best local ai models"]),
        new("take screenshot", "Capture the current screen to a local file.", CommandSafetyLevel.ConfirmationRequired, ["take screenshot"]),
        new("volume up", "Increase system volume.", CommandSafetyLevel.Safe, ["volume up"]),
        new("volume down", "Decrease system volume.", CommandSafetyLevel.Safe, ["volume down"]),
        new("mute volume", "Toggle system mute.", CommandSafetyLevel.Safe, ["mute volume", "unmute volume"]),
        new("sleep", "Put the computer to sleep.", CommandSafetyLevel.ConfirmationRequired, ["sleep computer"]),
        new("shutdown", "Schedule a local shutdown.", CommandSafetyLevel.ConfirmationRequired, ["shutdown computer"]),
        new("restart", "Schedule a local restart.", CommandSafetyLevel.ConfirmationRequired, ["restart computer"])
    ];

    public async Task<PcCommandExecutionResult> ExecuteAsync(
        string input,
        bool confirmed = false,
        CancellationToken cancellationToken = default)
    {
        RemoveExpiredConfirmations();

        if (confirmed)
        {
            var pending = FindPendingByInput(input);
            if (pending is not null)
            {
                return await ConfirmAsync(pending.Id, cancellationToken);
            }
        }

        var command = _parser.Parse(input);
        var safetyLevel = _safetyService.GetSafetyLevel(command.Action);
        await LogInteractionAsync(
            InteractionType.CommandParsing,
            "parse",
            safetyLevel == CommandSafetyLevel.Blocked ? InteractionStatus.Failed : InteractionStatus.Success,
            $"Parsed {command.Action} with safety {safetyLevel}.",
            command.OriginalInput,
            command.Target,
            cancellationToken);

        if (safetyLevel == CommandSafetyLevel.Blocked)
        {
            const string message = "Blocked unknown or unsupported system command.";
            await LogAsync(command, safetyLevel, CommandExecutionStatus.Blocked, message, cancellationToken);
            await LogInteractionAsync(InteractionType.Error, "blocked", InteractionStatus.Failed, message, command.OriginalInput, command.Target, cancellationToken);
            return new PcCommandExecutionResult(false, false, command.Action.ToString(), command.Target, message);
        }

        if (safetyLevel == CommandSafetyLevel.ConfirmationRequired && !confirmed)
        {
            var pending = CreatePending(command, safetyLevel);
            var message = $"Confirmation required before running {command.Action}: {DescribeTarget(command)}";
            await LogAsync(command, safetyLevel, CommandExecutionStatus.PendingConfirmation, message, cancellationToken);
            await LogInteractionAsync(InteractionType.Confirmation, "pending", InteractionStatus.Pending, message, command.OriginalInput, command.Target, cancellationToken);
            return new PcCommandExecutionResult(
                true,
                true,
                command.Action.ToString(),
                command.Target,
                message,
                pending.Id,
                pending.Id);
        }

        return await ExecuteKnownCommandAsync(command, safetyLevel, cancellationToken);
    }

    public async Task<PcCommandExecutionResult> ConfirmAsync(
        string confirmationId,
        CancellationToken cancellationToken = default)
    {
        RemoveExpiredConfirmations();

        if (!_pendingConfirmations.Remove(confirmationId, out var pending))
        {
            return new PcCommandExecutionResult(false, false, string.Empty, string.Empty, "Confirmation expired or not found.");
        }

        return await ExecuteKnownCommandAsync(pending.Command, pending.SafetyLevel, cancellationToken);
    }

    private async Task<PcCommandExecutionResult> ExecuteKnownCommandAsync(
        PcCommand command,
        CommandSafetyLevel safetyLevel,
        CancellationToken cancellationToken)
    {
        var message = await _pcControlService.ExecuteAsync(command, cancellationToken);
        var status = IsFailureMessage(message) ? CommandExecutionStatus.Failed : CommandExecutionStatus.Completed;
        await LogAsync(command, safetyLevel, status, message, cancellationToken);
        await LogInteractionAsync(
            InteractionType.CommandExecution,
            command.Action.ToString(),
            status == CommandExecutionStatus.Completed ? InteractionStatus.Success : InteractionStatus.Failed,
            message,
            command.OriginalInput,
            command.Target,
            cancellationToken);

        return new PcCommandExecutionResult(
            true,
            false,
            command.Action.ToString(),
            command.Target,
            message);
    }

    private PendingPcCommand CreatePending(PcCommand command, CommandSafetyLevel safetyLevel)
    {
        var pending = new PendingPcCommand
        {
            Id = Guid.NewGuid().ToString("N"),
            Command = command,
            SafetyLevel = safetyLevel,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(2)
        };

        _pendingConfirmations[pending.Id] = pending;
        return pending;
    }

    private PendingPcCommand? FindPendingByInput(string input)
    {
        var normalized = input.Trim();
        return _pendingConfirmations.Values.FirstOrDefault(pending =>
            pending.ExpiresAtUtc > DateTime.UtcNow &&
            pending.Command.OriginalInput.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

    private void RemoveExpiredConfirmations()
    {
        var expiredIds = _pendingConfirmations
            .Where(pair => pair.Value.ExpiresAtUtc <= DateTime.UtcNow)
            .Select(pair => pair.Key)
            .ToList();

        foreach (var id in expiredIds)
        {
            _pendingConfirmations.Remove(id);
        }
    }

    private async Task LogAsync(
        PcCommand command,
        CommandSafetyLevel safetyLevel,
        CommandExecutionStatus status,
        string message,
        CancellationToken cancellationToken)
    {
        await _logService.AddAsync(new PcCommandLogEntry
        {
            OriginalInput = command.OriginalInput,
            ParsedCommand = command.Action.ToString(),
            Target = command.Target,
            SafetyLevel = safetyLevel,
            Status = status,
            ResultMessage = message
        }, cancellationToken);
    }

    private static bool IsFailureMessage(string message)
    {
        return message.StartsWith("Could not", StringComparison.OrdinalIgnoreCase)
            || message.StartsWith("File was not found", StringComparison.OrdinalIgnoreCase)
            || message.StartsWith("Folder was not found", StringComparison.OrdinalIgnoreCase)
            || message.StartsWith("Screenshot failed", StringComparison.OrdinalIgnoreCase)
            || message.StartsWith("System action failed", StringComparison.OrdinalIgnoreCase)
            || message.StartsWith("Unsupported", StringComparison.OrdinalIgnoreCase);
    }

    private static string DescribeTarget(PcCommand command)
    {
        return string.IsNullOrWhiteSpace(command.Target) ? command.Action.ToString() : command.Target;
    }

    private Task LogInteractionAsync(
        InteractionType type,
        string stage,
        InteractionStatus status,
        string message,
        string input,
        string output,
        CancellationToken cancellationToken)
    {
        return _interactionLogService is null
            ? Task.CompletedTask
            : _interactionLogService.AddAsync(
                InteractionSource.Control,
                type,
                stage,
                status,
                message,
                input,
                output,
                cancellationToken: cancellationToken);
    }
}
