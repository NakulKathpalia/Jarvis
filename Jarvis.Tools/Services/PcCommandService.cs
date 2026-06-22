using Jarvis.Models;
using Jarvis.Security;

namespace Jarvis.Services;

public sealed class PcCommandService
{
    private readonly PcCommandParser _parser;
    private readonly CommandSafetyService _safetyService;
    private readonly CommandRiskClassifier _riskClassifier;
    private readonly SecurityService _securityService;
    private readonly CommandLogService _logService;
    private readonly InteractionLogService? _interactionLogService;
    private readonly IPcControlService _pcControlService;
    private readonly JarvisPersonalityService _personalityService;
    private readonly Dictionary<string, PendingPcCommand> _pendingConfirmations = new(StringComparer.OrdinalIgnoreCase);

    public PcCommandService(
        PcCommandParser parser,
        CommandSafetyService safetyService,
        CommandRiskClassifier riskClassifier,
        SecurityService securityService,
        CommandLogService logService,
        IPcControlService pcControlService,
        JarvisPersonalityService personalityService,
        InteractionLogService? interactionLogService = null)
    {
        _parser = parser;
        _safetyService = safetyService;
        _riskClassifier = riskClassifier;
        _securityService = securityService;
        _logService = logService;
        _pcControlService = pcControlService;
        _personalityService = personalityService;
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

        if (input.Trim().Equals(SecurityService.ConfirmationPhrase, StringComparison.Ordinal))
        {
            return await ConfirmLatestAsync(cancellationToken);
        }

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
        var riskLevel = _riskClassifier.Classify(command);
        var securityRequest = BuildSecurityRequest(command, riskLevel);
        var securityDecision = await _securityService.ValidateAsync(securityRequest, cancellationToken);
        await LogInteractionAsync(
            InteractionType.CommandParsing,
            "parse",
            securityDecision.Allowed || securityDecision.RequiresConfirmation ? InteractionStatus.Success : InteractionStatus.Failed,
            $"Parsed {command.Action} with risk {riskLevel}.",
            command.OriginalInput,
            command.Target,
            cancellationToken);

        if (!securityDecision.Allowed && !securityDecision.RequiresConfirmation)
        {
            var message = $"Blocked by security policy: {securityDecision.Reason}";
            await LogAsync(command, safetyLevel, CommandExecutionStatus.Blocked, message, cancellationToken);
            await LogInteractionAsync(InteractionType.Error, "blocked", InteractionStatus.Failed, message, command.OriginalInput, command.Target, cancellationToken);
            return new PcCommandExecutionResult(false, false, command.Action.ToString(), command.Target, message);
        }

        if (securityDecision.RequiresConfirmation && !confirmed)
        {
            var pending = CreatePending(command, safetyLevel, riskLevel);
            var message = $"This action is risky. Type: {SecurityService.ConfirmationPhrase}";
            await LogAsync(command, safetyLevel, CommandExecutionStatus.PendingConfirmation, message, cancellationToken);
            await LogInteractionAsync(InteractionType.Confirmation, "pending", InteractionStatus.Pending, message, command.OriginalInput, command.Target, cancellationToken);
            return new PcCommandExecutionResult(
                true,
                true,
                command.Action.ToString(),
                command.Target,
                message,
                SecurityService.ConfirmationPhrase,
                SecurityService.ConfirmationPhrase);
        }

        return await ExecuteKnownCommandAsync(command, safetyLevel, riskLevel, cancellationToken);
    }

    public async Task<PcCommandExecutionResult> ConfirmAsync(
        string confirmationId,
        CancellationToken cancellationToken = default)
    {
        RemoveExpiredConfirmations();

        if (!confirmationId.Equals(SecurityService.ConfirmationPhrase, StringComparison.Ordinal))
        {
            return new PcCommandExecutionResult(
                false,
                true,
                string.Empty,
                string.Empty,
                $"This action is risky. Type: {SecurityService.ConfirmationPhrase}",
                SecurityService.ConfirmationPhrase,
                SecurityService.ConfirmationPhrase);
        }

        return await ConfirmLatestAsync(cancellationToken);
    }

    private async Task<PcCommandExecutionResult> ConfirmLatestAsync(CancellationToken cancellationToken)
    {
        RemoveExpiredConfirmations();

        var pending = _pendingConfirmations.Values
            .Where(pending => pending.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(pending => pending.CreatedAtUtc)
            .FirstOrDefault();

        if (pending is null)
        {
            return new PcCommandExecutionResult(false, false, string.Empty, string.Empty, "Confirmation expired or not found.");
        }

        _pendingConfirmations.Remove(pending.Id);
        return await ExecuteKnownCommandAsync(pending.Command, pending.SafetyLevel, pending.RiskLevel, cancellationToken);
    }

    private async Task<PcCommandExecutionResult> ExecuteKnownCommandAsync(
        PcCommand command,
        CommandSafetyLevel safetyLevel,
        SecurityRiskLevel riskLevel,
        CancellationToken cancellationToken)
    {
        var executionMessage = await _pcControlService.ExecuteAsync(command, cancellationToken);
        var status = IsFailureMessage(executionMessage) ? CommandExecutionStatus.Failed : CommandExecutionStatus.Completed;
        var succeeded = status == CommandExecutionStatus.Completed;
        var responseMessage = _personalityService.FormatCommandResponse(command, succeeded, executionMessage);
        var securityRequest = BuildSecurityRequest(command, riskLevel);
        await _securityService.AuditExecutionAsync(
            securityRequest,
            succeeded,
            executionMessage,
            cancellationToken);
        await LogAsync(command, safetyLevel, status, executionMessage, cancellationToken);
        await LogInteractionAsync(
            InteractionType.CommandExecution,
            command.Action.ToString(),
            succeeded ? InteractionStatus.Success : InteractionStatus.Failed,
            executionMessage,
            command.OriginalInput,
            command.Target,
            cancellationToken);

        return new PcCommandExecutionResult(
            succeeded,
            false,
            command.Action.ToString(),
            command.Target,
            responseMessage);
    }

    private PendingPcCommand CreatePending(PcCommand command, CommandSafetyLevel safetyLevel)
    {
        return CreatePending(command, safetyLevel, _riskClassifier.Classify(command));
    }

    private PendingPcCommand CreatePending(PcCommand command, CommandSafetyLevel safetyLevel, SecurityRiskLevel riskLevel)
    {
        var pending = new PendingPcCommand
        {
            Id = Guid.NewGuid().ToString("N"),
            Command = command,
            SafetyLevel = safetyLevel,
            RiskLevel = riskLevel,
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

    private static SecurityRequest BuildSecurityRequest(PcCommand command, SecurityRiskLevel riskLevel)
    {
        return new SecurityRequest(
            "pc-command",
            command.OriginalInput,
            command.Action.ToString(),
            command.Target,
            riskLevel);
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
