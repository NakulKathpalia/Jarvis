namespace Jarvis.Models;

public enum PcControlAction
{
    Unknown,
    OpenApp,
    OpenWebsite,
    OpenFolder,
    OpenFile,
    BrowserSearch,
    TakeScreenshot,
    VolumeUp,
    VolumeDown,
    ToggleMute,
    Sleep,
    Shutdown,
    Restart
}

public enum CommandSafetyLevel
{
    Safe,
    ConfirmationRequired,
    Blocked
}

public enum CommandExecutionStatus
{
    PendingConfirmation,
    Completed,
    Failed,
    Blocked,
    Expired
}

public sealed record PcCommand(
    PcControlAction Action,
    string Target,
    string OriginalInput);

public sealed record PcCommandCatalogItem(
    string Command,
    string Description,
    CommandSafetyLevel SafetyLevel,
    IReadOnlyCollection<string> Examples);

public sealed class PcCommandLogEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string OriginalInput { get; set; } = string.Empty;
    public string ParsedCommand { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public CommandSafetyLevel SafetyLevel { get; set; } = CommandSafetyLevel.Blocked;
    public CommandExecutionStatus Status { get; set; } = CommandExecutionStatus.Blocked;
    public string ResultMessage { get; set; } = string.Empty;
}

public sealed class PendingPcCommand
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public PcCommand Command { get; set; } = new(PcControlAction.Unknown, string.Empty, string.Empty);
    public CommandSafetyLevel SafetyLevel { get; set; } = CommandSafetyLevel.ConfirmationRequired;
    public Jarvis.Security.SecurityRiskLevel RiskLevel { get; set; } = Jarvis.Security.SecurityRiskLevel.Dangerous;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddMinutes(2);
}

public sealed record PcCommandExecuteRequest(string Input);
public sealed record PcCommandConfirmRequest(string ConfirmationId);

public sealed record PcCommandExecutionResult(
    bool Handled,
    bool RequiresConfirmation,
    string Command,
    string Target,
    string Message,
    string? ConfirmationToken = null,
    string? ConfirmationId = null);

public sealed record AssistantInputRequest(string Message, string? ChatSessionId = null);

public sealed record AssistantConfirmRequest(string ConfirmationId, string? ChatSessionId = null);

public sealed record AssistantInputResponse(
    string Type,
    bool Handled,
    bool RequiresConfirmation,
    string Command,
    string Target,
    string Message,
    string? Response,
    string? ConfirmationId,
    ChatSession? Session);
