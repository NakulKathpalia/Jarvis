using Jarvis.Core;
using Jarvis.Models;

namespace Jarvis.Services;

public sealed class VoicePipelineService
{
    private readonly WhisperService _whisperService;
    private readonly WakeWordService _wakeWordService;
    private readonly PcCommandParser _pcCommandParser;
    private readonly PcCommandService _pcCommandService;
    private readonly Assistant _assistant;
    private readonly PiperService _piperService;
    private readonly SettingsService _settingsService;
    private readonly VoiceHistoryService _voiceHistoryService;

    private VoicePipelineStatus _status = new(
        VoicePipelineState.Idle,
        DateTime.UtcNow,
        string.Empty,
        string.Empty,
        "Voice pipeline is idle.");

    public VoicePipelineService(
        WhisperService whisperService,
        WakeWordService wakeWordService,
        PcCommandParser pcCommandParser,
        PcCommandService pcCommandService,
        Assistant assistant,
        PiperService piperService,
        SettingsService settingsService,
        VoiceHistoryService voiceHistoryService)
    {
        _whisperService = whisperService;
        _wakeWordService = wakeWordService;
        _pcCommandParser = pcCommandParser;
        _pcCommandService = pcCommandService;
        _assistant = assistant;
        _piperService = piperService;
        _settingsService = settingsService;
        _voiceHistoryService = voiceHistoryService;
    }

    public VoicePipelineStatus Status => _status;

    public async Task<VoicePipelineResult> ProcessAsync(
        IFormFile audio,
        bool requireWakeWord = false,
        CancellationToken cancellationToken = default)
    {
        if (audio.Length <= 0)
        {
            return await CompleteAsync(Fail(VoicePipelineState.Error, "Audio file is empty."), cancellationToken);
        }

        SetState(VoicePipelineState.Transcribing, "Transcribing local audio.");
        var transcription = await _whisperService.TranscribeAsync(audio, cancellationToken);
        if (!transcription.IsReady || !transcription.Succeeded)
        {
            return await CompleteAsync(Fail(VoicePipelineState.Error, transcription.Message), cancellationToken);
        }

        var transcript = transcription.Transcript.Trim();
        SetState(VoicePipelineState.WakeWordChecking, "Checking wake word.", transcript);
        var wakeWord = _wakeWordService.CheckTranscript(transcript);
        if (requireWakeWord && !wakeWord.Detected)
        {
            return await CompleteAsync(new VoicePipelineResult(
                transcript,
                false,
                false,
                string.Empty,
                false,
                string.Empty,
                string.Empty,
                VoicePipelineState.Completed,
                false,
                wakeWord.Message), cancellationToken);
        }

        var command = _pcCommandParser.Parse(transcript);
        if (command.Action != PcControlAction.Unknown)
        {
            return await ProcessCommandAsync(transcript, wakeWord.Detected, cancellationToken);
        }

        return await ProcessAiFallbackAsync(transcript, wakeWord.Detected, cancellationToken);
    }

    public async Task<VoicePipelineResult> ConfirmAsync(
        string confirmationId,
        CancellationToken cancellationToken = default)
    {
        SetState(VoicePipelineState.ExecutingCommand, "Executing confirmed voice command.");
        var result = await _pcCommandService.ConfirmAsync(confirmationId, cancellationToken);
        var pipelineResult = new VoicePipelineResult(
            string.Empty,
            false,
            result.Handled,
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
        bool wakeWordDetected,
        CancellationToken cancellationToken)
    {
        SetState(VoicePipelineState.CommandDetected, "Local command detected.", transcript);
        var commandResult = await _pcCommandService.ExecuteAsync(transcript, cancellationToken: cancellationToken);

        if (commandResult.RequiresConfirmation)
        {
            var result = new VoicePipelineResult(
                transcript,
                wakeWordDetected,
                true,
                commandResult.Command,
                true,
                string.Empty,
                string.Empty,
                VoicePipelineState.AwaitingConfirmation,
                true,
                commandResult.Message,
                commandResult.ConfirmationId);

            return await CompleteAsync(result, cancellationToken, persistHistory: false);
        }

        var completed = new VoicePipelineResult(
            transcript,
            wakeWordDetected,
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
        bool wakeWordDetected,
        CancellationToken cancellationToken)
    {
        SetState(VoicePipelineState.GeneratingAIResponse, "Generating AI response.", transcript);
        var response = await _assistant.GenerateResponseAsync(transcript, cancellationToken: cancellationToken);
        var audioUrl = string.Empty;
        var message = "AI response generated.";

        if (_settingsService.Current.AutoSpeakResponses && !string.IsNullOrWhiteSpace(response))
        {
            SetState(VoicePipelineState.Speaking, "Generating Piper speech.", transcript, response);
            var speech = await _piperService.SpeakAsync(response, cancellationToken);
            audioUrl = speech.AudioUrl;
            message = speech.Succeeded ? "AI response generated with speech." : speech.Message;
        }

        return await CompleteAsync(new VoicePipelineResult(
            transcript,
            wakeWordDetected,
            false,
            string.Empty,
            false,
            response,
            audioUrl,
            VoicePipelineState.Completed,
            !string.IsNullOrWhiteSpace(response),
            message), cancellationToken);
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
}
