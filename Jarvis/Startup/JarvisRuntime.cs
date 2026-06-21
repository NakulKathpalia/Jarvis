using Jarvis.Auth;
using Jarvis.Commands;
using Jarvis.ConnectedApps;
using Jarvis.Core;
using Jarvis.Data;
using Jarvis.Memory;
using Jarvis.Security;
using Jarvis.Services;
using Jarvis.Voice;

namespace Jarvis.Startup;

public sealed class JarvisRuntime
{
    public required PathResolver PathResolver { get; init; }
    public required PlatformService PlatformService { get; init; }
    public required SettingsService SettingsService { get; init; }
    public required MemoryService MemoryService { get; init; }
    public required ChatHistoryService ChatHistoryService { get; init; }
    public required ChatSessionService ChatSessionService { get; init; }
    public required CommandLogService CommandLogService { get; init; }
    public required InteractionLogService InteractionLogService { get; init; }
    public required VoiceHistoryService VoiceHistoryService { get; init; }
    public required VoiceSettingsService VoiceSettingsService { get; init; }
    public required SpeechToTextService SpeechToTextService { get; init; }
    public required VoiceActivityDetector VoiceActivityDetector { get; init; }
    public required VoiceCommandProcessor VoiceCommandProcessor { get; init; }
    public required PermissionService PermissionService { get; init; }
    public required OllamaService OllamaService { get; init; }
    public required WhisperService WhisperService { get; init; }
    public required PiperService PiperService { get; init; }
    public required Jarvis.Services.WakeWordService WakeWordService { get; init; }
    public required FileIndexService FileIndexService { get; init; }
    public required PcCommandParser PcCommandParser { get; init; }
    public required PcCommandService PcCommandService { get; init; }
    public required VoiceCommandService VoiceCommandService { get; init; }
    public required SettingsValidationService SettingsValidationService { get; init; }
    public required IAuthService AuthService { get; init; }
    public required IConnectedAppService ConnectedAppService { get; init; }
    public required CommandManager CommandManager { get; init; }
    public required Assistant Assistant { get; init; }
    public required VoicePipelineService VoicePipelineService { get; init; }
    public required CommandRouter Router { get; init; }
    public required HttpClient HttpClient { get; init; }
}
