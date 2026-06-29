namespace Jarvis.VoiceAgent;

using Jarvis.Core.Agents.Voice.Models;
using Jarvis.Core.Agents.Voice.Skills;
using Jarvis.Core.Agents.Voice.Tools;
using Jarvis.Core.Brain.Analysis;
using Jarvis.Core.Brain.Intent;
using Jarvis.Core.Brain.Models;
using Jarvis.Core.Brain.Routing;
using Jarvis.Core.Brain.Services;
using Jarvis.Core.Brain.Workflow;
using Jarvis.Core.Framework.Agents;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Planner;
using Jarvis.Core.Framework.Registry;
using Jarvis.Core.Framework.Routing;
using Jarvis.Models;
using Jarvis.Services;
using Jarvis.Voice;
using Microsoft.AspNetCore.Http;
using CoreVoiceAgent = Jarvis.Core.Agents.Voice.VoiceAgent;

public sealed class VoiceAgentRuntime
{
    private readonly AgentRegistry registry = new();
    private readonly TaskPipeline pipeline;
    private readonly VoicePipelineService voicePipelineService;
    private readonly Brain brain;
    private readonly object gate = new();
    private VoiceAgentStatus status = new();
    private ExecutionPlan? lastPlan;

    public VoiceAgentRuntime(
        VoicePipelineService voicePipelineService,
        WhisperService whisperService,
        TextToSpeechService textToSpeechService,
        Jarvis.Services.WakeWordService wakeWordService,
        VoiceCommandService voiceCommandService)
    {
        this.voicePipelineService = voicePipelineService;

        var pipelineTool = new VoicePipelineTool(new VoicePipelineBackendAdapter(voicePipelineService));
        var whisperTool = new WhisperTool(new WhisperBackendAdapter(whisperService));
        var playbackTool = new VoicePlaybackTool(new VoicePlaybackBackendAdapter(textToSpeechService));
        var wakeWordTool = new WakeWordTool(new WakeWordBackendAdapter(wakeWordService));
        var commandTool = new VoiceCommandTool(new VoiceCommandBackendAdapter(voiceCommandService));
        var piperTool = new PiperTool(new PiperBackendAdapter(textToSpeechService));

        registry.Register(new CoreVoiceAgent([
            new ListenSkill(pipelineTool),
            new TranscribeSkill(whisperTool),
            new WakeWordSkill(wakeWordTool),
            new SpeakSkill(playbackTool),
            new VoiceCommandSkill(commandTool)
        ]));

        var agentManager = new AgentManager(
            new TaskPlanner(),
            registry,
            new ContextManager(),
            new ToolExecutor());
        pipeline = new TaskPipeline(agentManager);
        brain = new Brain(
            new IntentAnalyzer(),
            new TaskAnalyzer(),
            new WorkflowPlanner(),
            new AgentSelector(),
            new SkillSelector(),
            new ToolSelector(),
            new ModelRouter());

        _ = piperTool;
    }

    public VoiceAgentStatus Status
    {
        get
        {
            lock (gate)
            {
                var snapshot = CopyStatus(status);
                snapshot.CurrentPipelineState = voicePipelineService.Status.State.ToString();
                return snapshot;
            }
        }
    }

    public ExecutionPlan? LastPlan => lastPlan;

    public async Task<VoicePipelineResult> ProcessAsync(
        IFormFile audio,
        bool requireWakeWord,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest("voice pipeline", "Process", ["ListenSkill"]);
        request.Parameters["Audio"] = audio;
        request.Parameters["RequireWakeWord"] = requireWakeWord;
        return await ExecuteForValueAsync<VoicePipelineResult>(request, cancellationToken);
    }

    public async Task<VoicePipelineResult> ConfirmAsync(
        string confirmationId,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest("voice confirmation", "Confirm", ["ListenSkill"]);
        request.Parameters["ConfirmationId"] = confirmationId;
        return await ExecuteForValueAsync<VoicePipelineResult>(request, cancellationToken);
    }

    public async Task<WhisperTranscriptionResult> TranscribeAsync(
        IFormFile audio,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest("voice transcribe", "Transcribe", ["TranscribeSkill"]);
        request.Parameters["Audio"] = audio;
        return await ExecuteForValueAsync<WhisperTranscriptionResult>(request, cancellationToken);
    }

    public async Task<WakeWordDetectionResult> CheckWakeWordAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(transcript, "WakeCheck", ["WakeWordSkill"]);
        request.Parameters["Transcript"] = transcript;
        return await ExecuteForValueAsync<WakeWordDetectionResult>(request, cancellationToken);
    }

    public async Task<TextToSpeechResult> SpeakAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(text, "Speak", ["SpeakSkill"]);
        request.Parameters["Text"] = text;
        return await ExecuteForValueAsync<TextToSpeechResult>(request, cancellationToken);
    }

    public async Task StopSpeakingAsync(CancellationToken cancellationToken = default)
    {
        var request = CreateRequest("stop speaking", "StopSpeaking", ["SpeakSkill"]);
        await ExecuteAsync(request, cancellationToken);
    }

    public async Task<VoiceCommandResult> ExecuteCommandAsync(
        string transcript,
        bool confirmed,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(transcript, "Command", ["VoiceCommandSkill"]);
        request.Parameters["Transcript"] = transcript;
        request.Parameters["Confirmed"] = confirmed;
        return await ExecuteForValueAsync<VoiceCommandResult>(request, cancellationToken);
    }

    public void CancelCurrentTask()
    {
        UpdateStatus("Cancelled", "Ready", string.Empty, "Voice task cancellation requested.");
    }

    public void StopListening()
    {
        UpdateStatus("ListeningStopped", "Ready", string.Empty, string.Empty);
    }

    private TaskRequest CreateRequest(string input, string operation, IReadOnlyCollection<string> skills)
    {
        lastPlan = brain.Plan($"voice {input}", registry);
        return new TaskRequest
        {
            TaskType = "Voice",
            Input = input,
            Parameters =
            {
                ["VoiceOperation"] = operation,
                ["VoiceSkills"] = skills
            }
        };
    }

    private async Task<T> ExecuteForValueAsync<T>(
        TaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await ExecuteAsync(request, cancellationToken);
        var value = result.ToolResults.LastOrDefault()?.Value;
        if (value is T typed)
        {
            return typed;
        }

        throw new InvalidOperationException(result.ErrorMessage.Length == 0
            ? $"Voice Agent did not return {typeof(T).Name}."
            : result.ErrorMessage);
    }

    private async Task<TaskResult> ExecuteAsync(TaskRequest request, CancellationToken cancellationToken)
    {
        UpdateStatus("Running", "Ready", request.RequestId, string.Empty);
        var result = await pipeline.ExecuteAsync(request, cancellationToken);
        UpdateStatus(
            result.Succeeded ? "Completed" : "Error",
            result.Succeeded ? "Ready" : "Error",
            string.Empty,
            result.ErrorMessage);
        return result;
    }

    private void UpdateStatus(string state, string health, string activeTask, string lastError)
    {
        lock (gate)
        {
            status = new VoiceAgentStatus
            {
                CurrentState = state,
                Health = health,
                ActiveTask = activeTask,
                LastError = lastError,
                LastExecutionTimeUtc = DateTimeOffset.UtcNow,
                CurrentPipelineState = voicePipelineService.Status.State.ToString()
            };
        }
    }

    private static VoiceAgentStatus CopyStatus(VoiceAgentStatus source)
    {
        return new VoiceAgentStatus
        {
            CurrentState = source.CurrentState,
            Health = source.Health,
            ActiveTask = source.ActiveTask,
            LastError = source.LastError,
            LastExecutionTimeUtc = source.LastExecutionTimeUtc,
            CurrentPipelineState = source.CurrentPipelineState
        };
    }
}
