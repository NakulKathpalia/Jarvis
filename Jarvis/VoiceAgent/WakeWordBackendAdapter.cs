namespace Jarvis.VoiceAgent;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Services;

public sealed class WakeWordBackendAdapter : IWakeWordBackend
{
    private readonly WakeWordService wakeWordService;

    public WakeWordBackendAdapter(WakeWordService wakeWordService)
    {
        this.wakeWordService = wakeWordService;
    }

    public Task<ToolResult> CheckAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var transcript = context.Request.Parameters.TryGetValue("Transcript", out var value)
            ? value?.ToString() ?? string.Empty
            : context.Request.Input;
        var result = wakeWordService.CheckTranscript(transcript);
        return Task.FromResult(new ToolResult
        {
            ToolName = "WakeWordTool",
            Succeeded = true,
            Output = result.Message,
            Value = result
        });
    }
}
