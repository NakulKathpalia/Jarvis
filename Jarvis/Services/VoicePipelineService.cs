using Jarvis.Core;
using Jarvis.Models;
using Jarvis.Security;
using Jarvis.Voice;

namespace Jarvis.Services;

public sealed class VoicePipelineService
{
    private readonly VoiceSettingsService _voiceSettingsService;
    private readonly VoiceActivityDetector _voiceActivityDetector;
    private readonly SpeechToTextService _speechToTextService;
    private readonly VoiceCommandProcessor _voiceCommandProcessor;
    private readonly Assistant _assistant;
    private readonly VoiceHistoryService _voiceHistoryService;
    private readonly InteractionLogService? _interactionLogService;

    private VoicePipelineStatus _status = new(
        VoicePipelineState.Idle,
        DateTime.UtcNow,
        string.Empty,
        string.Empty,
        "Voice pipeline is idle.");

    public VoicePipelineService(
        VoiceSettingsService voiceSettingsService,
        VoiceActivityDetector voiceActivityDetector,
        SpeechToTextService speechToTextService,
        VoiceCommandProcessor voiceCommandProcessor,
        Assistant assistant,
        VoiceHistoryService voiceHistoryService,
        InteractionLogService? interactionLogService = null)
    {
        _voiceSettingsService = voiceSettingsService;
        _voiceActivityDetector = voiceActivityDetector;
        _speechToTextService = speechToTextService;
        _voiceCommandProcessor = voiceCommandProcessor;
        _assistant = assistant;
        _voiceHistoryService = voiceHistoryService;
        _interactionLogService = interactionLogService;
    }

    public VoicePipelineStatus Status => _status;

    public async Task<VoicePipelineResult> ProcessAsync(
        IFormFile audio,
        bool requireWakeWord = false,
        CancellationToken cancellationToken = default)
    {
        if (!_voiceSettingsService.PushToTalkEnabled)
        {
            return await CompleteAsync(Fail(VoicePipelineState.Error, $"{_voiceSettingsService.Mode} is reserved for a future sprint. Use PushToTalk."), cancellationToken);
        }

        if (audio.Length <= 0)
        {
            return await CompleteAsync(Fail(VoicePipelineState.Error, "Audio file is empty."), cancellationToken);
        }

        SetState(VoicePipelineState.Listening, "Detecting speech activity.");
        var activity = _voiceActivityDetector.Detect(audio);
        await LogAsync(
            InteractionType.VoiceRecording,
            "voice-activity",
            activity.SpeechDetected ? InteractionStatus.Success : InteractionStatus.Skipped,
            activity.Message,
            $"{audio.FileName} ({audio.Length} bytes)",
            cancellationToken: cancellationToken);
        if (!activity.SpeechDetected)
        {
            return await CompleteAsync(Fail(VoicePipelineState.Completed, activity.Message), cancellationToken);
        }

        SetState(VoicePipelineState.Transcribing, "Transcribing local audio.");
        var transcription = await _speechToTextService.TranscribeAsync(audio, cancellationToken);
        if (!transcription.IsReady || !transcription.Succeeded)
        {
            return await CompleteAsync(Fail(VoicePipelineState.Error, transcription.Message), cancellationToken);
        }

        var transcript = transcription.Transcript.Trim();
        var command = _voiceCommandProcessor.Parse(transcript);
        await LogAsync(InteractionType.CommandParsing, "voice-parser", command.Action == PcControlAction.Unknown ? InteractionStatus.Skipped : InteractionStatus.Success, command.Action == PcControlAction.Unknown ? "No voice command detected." : $"Detected voice command {command.Action}.", transcript, command.Target, cancellationToken: cancellationToken);
        if (command.Action != PcControlAction.Unknown)
        {
            return await ProcessCommandAsync(transcript, cancellationToken);
        }

        return await ProcessAiFallbackAsync(transcript, cancellationToken);
    }

    public async Task<VoicePipelineResult> ConfirmAsync(
        string confirmationId,
        CancellationToken cancellationToken = default)
    {
        SetState(VoicePipelineState.ExecutingCommand, "Executing confirmed voice command.");
        var result = await _voiceCommandProcessor.ConfirmAsync(confirmationId, cancellationToken);
        var pipelineResult = new VoicePipelineResult(
            string.Empty,
            false,
            result.Handled || result.RequiresConfirmation,
            result.Command,
            false,
            string.Empty,
            string.Empty,
            result.Handled ? VoicePipelineState.Completed : VoicePipelineState.Error,
            result.Handled,
            result.Message);

        return await CompleteAsync(pipelineResult, cancellationToken);
    }

    private async Task<VoicePipelineResult> ProcessCommandAsync(
        string transcript,
        CancellationToken cancellationToken)
    {
        SetState(VoicePipelineState.CommandDetected, "Local command detected.", transcript);
        await LogAsync(InteractionType.CommandExecution, "voice-command-start", InteractionStatus.Started, "Executing or preparing local voice command.", transcript, cancellationToken: cancellationToken);
        var commandResult = await _voiceCommandProcessor.TryExecuteAsync(transcript, cancellationToken: cancellationToken);

        if (commandResult.RequiresConfirmation)
        {
            var result = new VoicePipelineResult(
                transcript,
                false,
                true,
                commandResult.Command,
                true,
                string.Empty,
                string.Empty,
                VoicePipelineState.AwaitingConfirmation,
                true,
                commandResult.Message,
                commandResult.ConfirmationValue);

            return await CompleteAsync(result, cancellationToken, persistHistory: false);
        }

        var completed = new VoicePipelineResult(
            transcript,
            false,
            commandResult.Handled,
            commandResult.Command,
            false,
            string.Empty,
            string.Empty,
            commandResult.Handled ? VoicePipelineState.Completed : VoicePipelineState.Error,
            commandResult.Handled,
            commandResult.Message);

        return await CompleteAsync(completed, cancellationToken);
    }

    private async Task<VoicePipelineResult> ProcessAiFallbackAsync(
        string transcript,
        CancellationToken cancellationToken)
    {
        SetState(VoicePipelineState.GeneratingAIResponse, "Generating AI response.", transcript);
        await LogAsync(InteractionType.AiFallback, "voice-fallback", InteractionStatus.Started, "Voice input routed to AI fallback.", transcript, cancellationToken: cancellationToken);
        var response = await _assistant.GenerateResponseAsync(transcript, cancellationToken: cancellationToken);

        return await CompleteAsync(new VoicePipelineResult(
            transcript,
            false,
            false,
            string.Empty,
            false,
            response,
            string.Empty,
            VoicePipelineState.Completed,
            !string.IsNullOrWhiteSpace(response),
            "AI response generated."), cancellationToken);
    }

    private async Task<VoicePipelineResult> CompleteAsync(
        VoicePipelineResult result,
        CancellationToken cancellationToken,
        bool persistHistory = true)
    {
        SetState(result.State, result.Message, result.Transcript, result.AiResponse);
        if (persistHistory)
        {
            await _voiceHistoryService.AddAsync(result, cancellationToken);
        }

        return result;
    }

    private void SetState(
        VoicePipelineState state,
        string message,
        string? transcript = null,
        string? aiResponse = null)
    {
        _status = new VoicePipelineStatus(
            state,
            DateTime.UtcNow,
            transcript ?? _status.LastTranscript,
            aiResponse ?? _status.LastAiResponse,
            message);
    }

    private static VoicePipelineResult Fail(VoicePipelineState state, string message)
    {
        return new VoicePipelineResult(
            string.Empty,
            false,
            false,
            string.Empty,
            false,
            string.Empty,
            string.Empty,
            state,
            false,
            message);
    }

    private Task LogAsync(
        InteractionType type,
        string stage,
        InteractionStatus status,
        string message,
        string input = "",
        string output = "",
        string error = "",
        CancellationToken cancellationToken = default)
    {
        return _interactionLogService is null
            ? Task.CompletedTask
            : _interactionLogService.AddAsync(
                InteractionSource.Voice,
                type,
                stage,
                status,
                message,
                input,
                output,
                error,
                cancellationToken: cancellationToken);
    }
}
