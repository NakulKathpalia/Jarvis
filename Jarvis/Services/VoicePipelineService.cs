using System.Diagnostics;
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
        var trace = VoicePipelineTrace.Start(audio.Length);

        if (!_voiceSettingsService.PushToTalkEnabled)
        {
            trace.Fail($"{_voiceSettingsService.Mode} is reserved for a future sprint. Use PushToTalk.");
            return await CompleteAsync(Fail(trace, VoicePipelineState.Error, trace.FailureReason), cancellationToken);
        }

        if (audio.Length <= 0)
        {
            trace.Fail("Audio file is empty.");
            return await CompleteAsync(Fail(trace, VoicePipelineState.Error, trace.FailureReason), cancellationToken);
        }

        try
        {
            SetState(VoicePipelineState.Listening, "Audio received from push-to-talk capture.", trace);
            trace.CompleteStage("AudioUpload");

            SetState(VoicePipelineState.Recording, "Audio capture uploaded for processing.", trace);
            await LogAsync(
                InteractionType.VoiceRecording,
                "audio-capture-uploaded",
                InteractionStatus.Success,
                "Voice audio uploaded to backend.",
                audio.FileName,
                $"{audio.Length} bytes",
                cancellationToken: cancellationToken);

            SetState(VoicePipelineState.Processing, "Detecting speech activity.", trace);
            var activity = _voiceActivityDetector.Detect(audio);
            trace.CompleteStage("VoiceActivityDetection");
            await LogAsync(
                InteractionType.VoiceRecording,
                "voice-activity",
                activity.SpeechDetected ? InteractionStatus.Success : InteractionStatus.Skipped,
                activity.Message,
                $"{audio.FileName} ({audio.Length} bytes)",
                cancellationToken: cancellationToken);
            if (!activity.SpeechDetected)
            {
                trace.Fail(activity.Message);
                return await CompleteAsync(Fail(trace, VoicePipelineState.Error, activity.Message), cancellationToken);
            }

            SetState(VoicePipelineState.Transcribing, "Transcribing local audio with configured speech-to-text engine.", trace);
            var sttWatch = Stopwatch.StartNew();
            var transcription = await _speechToTextService.TranscribeAsync(audio, cancellationToken);
            sttWatch.Stop();
            trace.SttDurationMs = sttWatch.ElapsedMilliseconds;
            trace.SttDevice = transcription.Device;
            trace.CompleteStage("SpeechToText");
            if (!transcription.IsReady || !transcription.Succeeded)
            {
                trace.Fail(transcription.Message);
                return await CompleteAsync(Fail(trace, VoicePipelineState.Error, transcription.Message), cancellationToken);
            }

            var transcript = transcription.Transcript.Trim();
            trace.Transcript = transcript;
            SetState(VoicePipelineState.Understanding, "Understanding transcript and checking for commands.", trace, transcript);
            var command = _voiceCommandProcessor.Parse(transcript);
            trace.CommandDetected = command.Action != PcControlAction.Unknown;
            trace.CommandName = trace.CommandDetected ? command.Action.ToString() : string.Empty;
            trace.CompleteStage("CommandParsing");
            await LogAsync(
                InteractionType.CommandParsing,
                "voice-parser",
                command.Action == PcControlAction.Unknown ? InteractionStatus.Skipped : InteractionStatus.Success,
                command.Action == PcControlAction.Unknown ? "No voice command detected." : $"Detected voice command {command.Action}.",
                transcript,
                command.Target,
                cancellationToken: cancellationToken);
            if (command.Action != PcControlAction.Unknown)
            {
                return await ProcessCommandAsync(transcript, trace, cancellationToken);
            }

            return await ProcessAiFallbackAsync(transcript, trace, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            trace.Fail(ex.Message);
            await LogAsync(
                InteractionType.Error,
                "voice-pipeline-exception",
                InteractionStatus.Failed,
                "Voice pipeline failed.",
                audio.FileName,
                trace.LastCompletedStage,
                ex.Message,
                cancellationToken);
            return await CompleteAsync(Fail(trace, VoicePipelineState.Error, $"Voice pipeline failed: {ex.Message}"), cancellationToken);
        }
    }

    public async Task<VoicePipelineResult> ConfirmAsync(
        string confirmationId,
        CancellationToken cancellationToken = default)
    {
        var trace = VoicePipelineTrace.Start(0);
        SetState(VoicePipelineState.ExecutingCommand, "Executing confirmed voice command.", trace);
        var commandWatch = Stopwatch.StartNew();
        var result = await _voiceCommandProcessor.ConfirmAsync(confirmationId, cancellationToken);
        commandWatch.Stop();
        trace.CommandDurationMs = commandWatch.ElapsedMilliseconds;
        trace.CommandDetected = result.Handled || result.RequiresConfirmation;
        trace.CommandExecuted = result.Handled;
        trace.CommandName = result.Command;
        trace.CompleteStage("CommandConfirmation");
        if (!result.Handled)
        {
            trace.Fail(result.Message);
        }
        else
        {
            trace.End();
        }

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
            result.Message,
            VoiceSessionId: trace.SessionId,
            StartedAtUtc: trace.StartedAtUtc,
            EndedAtUtc: trace.EndedAtUtc,
            ProcessingDurationMs: trace.ProcessingDurationMs,
            CommandDurationMs: trace.CommandDurationMs,
            FailureReason: trace.FailureReason,
            LastCompletedStage: trace.LastCompletedStage,
            CommandExecuted: trace.CommandExecuted);

        return await CompleteAsync(pipelineResult, cancellationToken);
    }

    private async Task<VoicePipelineResult> ProcessCommandAsync(
        string transcript,
        VoicePipelineTrace trace,
        CancellationToken cancellationToken)
    {
        SetState(VoicePipelineState.ExecutingCommand, "Local command detected. Running secure command path.", trace, transcript);
        await LogAsync(InteractionType.CommandExecution, "voice-command-start", InteractionStatus.Started, "Executing or preparing local voice command.", transcript, cancellationToken: cancellationToken);
        var commandWatch = Stopwatch.StartNew();
        var commandResult = await _voiceCommandProcessor.TryExecuteAsync(transcript, cancellationToken: cancellationToken);
        commandWatch.Stop();
        trace.CommandDurationMs = commandWatch.ElapsedMilliseconds;
        trace.CommandName = commandResult.Command;
        trace.CommandExecuted = commandResult.Handled && !commandResult.RequiresConfirmation;
        trace.CompleteStage("CommandExecution");

        if (commandResult.RequiresConfirmation)
        {
            trace.End();
            var result = new VoicePipelineResult(
                transcript,
                false,
                true,
                commandResult.Command,
                true,
                string.Empty,
                string.Empty,
                VoicePipelineState.ExecutingCommand,
                true,
                commandResult.Message,
                commandResult.ConfirmationValue,
                VoiceSessionId: trace.SessionId,
                StartedAtUtc: trace.StartedAtUtc,
                EndedAtUtc: trace.EndedAtUtc,
                AudioSizeBytes: trace.AudioSizeBytes,
                ProcessingDurationMs: trace.ProcessingDurationMs,
                SttDurationMs: trace.SttDurationMs,
                CommandDurationMs: trace.CommandDurationMs,
                LastCompletedStage: trace.LastCompletedStage,
                SttDevice: trace.SttDevice,
                CommandExecuted: false);

            return await CompleteAsync(result, cancellationToken, persistHistory: false);
        }

        if (!commandResult.Handled)
        {
            trace.Fail(commandResult.Message);
        }
        else
        {
            trace.End();
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
            commandResult.Message,
            VoiceSessionId: trace.SessionId,
            StartedAtUtc: trace.StartedAtUtc,
            EndedAtUtc: trace.EndedAtUtc,
            AudioSizeBytes: trace.AudioSizeBytes,
            ProcessingDurationMs: trace.ProcessingDurationMs,
            SttDurationMs: trace.SttDurationMs,
            CommandDurationMs: trace.CommandDurationMs,
            FailureReason: trace.FailureReason,
            LastCompletedStage: trace.LastCompletedStage,
            SttDevice: trace.SttDevice,
            CommandExecuted: trace.CommandExecuted);

        return await CompleteAsync(completed, cancellationToken);
    }

    private async Task<VoicePipelineResult> ProcessAiFallbackAsync(
        string transcript,
        VoicePipelineTrace trace,
        CancellationToken cancellationToken)
    {
        SetState(VoicePipelineState.Understanding, "Generating assistant response.", trace, transcript);
        await LogAsync(InteractionType.AiFallback, "voice-fallback", InteractionStatus.Started, "Voice input routed to AI fallback.", transcript, cancellationToken: cancellationToken);
        var response = await _assistant.GenerateResponseAsync(transcript, cancellationToken: cancellationToken);
        trace.AiResponse = response;
        trace.CompleteStage("AssistantResponse");
        if (string.IsNullOrWhiteSpace(response))
        {
            trace.Fail("Assistant returned an empty response.");
        }
        else
        {
            trace.End();
        }

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
            "AI response generated.",
            VoiceSessionId: trace.SessionId,
            StartedAtUtc: trace.StartedAtUtc,
            EndedAtUtc: trace.EndedAtUtc,
            AudioSizeBytes: trace.AudioSizeBytes,
            ProcessingDurationMs: trace.ProcessingDurationMs,
            SttDurationMs: trace.SttDurationMs,
            FailureReason: trace.FailureReason,
            LastCompletedStage: trace.LastCompletedStage,
            SttDevice: trace.SttDevice), cancellationToken);
    }

    private async Task<VoicePipelineResult> CompleteAsync(
        VoicePipelineResult result,
        CancellationToken cancellationToken,
        bool persistHistory = true)
    {
        SetState(result);
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

    private void SetState(VoicePipelineResult result)
    {
        _status = new VoicePipelineStatus(
            result.State,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(result.Transcript) ? _status.LastTranscript : result.Transcript,
            string.IsNullOrWhiteSpace(result.AiResponse) ? _status.LastAiResponse : result.AiResponse,
            result.Message,
            result.VoiceSessionId,
            result.StartedAtUtc,
            result.EndedAtUtc,
            result.AudioSizeBytes,
            result.RecordingDurationMs,
            result.ProcessingDurationMs,
            result.SttDurationMs,
            result.CommandDurationMs,
            result.CommandDetected,
            result.CommandExecuted,
            result.CommandName,
            result.FailureReason,
            result.LastCompletedStage,
            "Browser capture uploaded",
            result.SttDevice);
    }

    private void SetState(
        VoicePipelineState state,
        string message,
        VoicePipelineTrace trace,
        string? transcript = null,
        string? aiResponse = null)
    {
        trace.RefreshProcessingDuration();
        _status = new VoicePipelineStatus(
            state,
            DateTime.UtcNow,
            transcript ?? trace.Transcript ?? _status.LastTranscript,
            aiResponse ?? trace.AiResponse ?? _status.LastAiResponse,
            message,
            trace.SessionId,
            trace.StartedAtUtc,
            trace.EndedAtUtc,
            trace.AudioSizeBytes,
            trace.RecordingDurationMs,
            trace.ProcessingDurationMs,
            trace.SttDurationMs,
            trace.CommandDurationMs,
            trace.CommandDetected,
            trace.CommandExecuted,
            trace.CommandName,
            trace.FailureReason,
            trace.LastCompletedStage,
            "Browser capture uploaded",
            trace.SttDevice);
    }

    private static VoicePipelineResult Fail(VoicePipelineTrace trace, VoicePipelineState state, string message)
    {
        trace.Fail(message);
        trace.End();
        return new VoicePipelineResult(
            trace.Transcript ?? string.Empty,
            false,
            trace.CommandDetected,
            trace.CommandName,
            false,
            string.Empty,
            string.Empty,
            state,
            false,
            message,
            VoiceSessionId: trace.SessionId,
            StartedAtUtc: trace.StartedAtUtc,
            EndedAtUtc: trace.EndedAtUtc,
            AudioSizeBytes: trace.AudioSizeBytes,
            RecordingDurationMs: trace.RecordingDurationMs,
            ProcessingDurationMs: trace.ProcessingDurationMs,
            SttDurationMs: trace.SttDurationMs,
            CommandDurationMs: trace.CommandDurationMs,
            FailureReason: trace.FailureReason,
            LastCompletedStage: trace.LastCompletedStage,
            SttDevice: trace.SttDevice,
            CommandExecuted: trace.CommandExecuted);
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

    private sealed class VoicePipelineTrace
    {
        private readonly Stopwatch _processingWatch = Stopwatch.StartNew();

        private VoicePipelineTrace(long audioSizeBytes)
        {
            AudioSizeBytes = audioSizeBytes;
        }

        public string SessionId { get; } = Guid.NewGuid().ToString("N");
        public DateTime StartedAtUtc { get; } = DateTime.UtcNow;
        public DateTime? EndedAtUtc { get; private set; }
        public long AudioSizeBytes { get; }
        public long RecordingDurationMs { get; set; }
        public long ProcessingDurationMs { get; private set; }
        public long SttDurationMs { get; set; }
        public long CommandDurationMs { get; set; }
        public string FailureReason { get; private set; } = string.Empty;
        public string LastCompletedStage { get; private set; } = "Started";
        public string SttDevice { get; set; } = string.Empty;
        public string? Transcript { get; set; }
        public string? AiResponse { get; set; }
        public bool CommandDetected { get; set; }
        public bool CommandExecuted { get; set; }
        public string CommandName { get; set; } = string.Empty;

        public static VoicePipelineTrace Start(long audioSizeBytes) => new(audioSizeBytes);

        public void CompleteStage(string stage)
        {
            LastCompletedStage = stage;
            RefreshProcessingDuration();
        }

        public void Fail(string reason)
        {
            FailureReason = reason;
            End();
        }

        public void End()
        {
            EndedAtUtc ??= DateTime.UtcNow;
            RefreshProcessingDuration();
        }

        public void RefreshProcessingDuration()
        {
            ProcessingDurationMs = _processingWatch.ElapsedMilliseconds;
        }
    }
}
