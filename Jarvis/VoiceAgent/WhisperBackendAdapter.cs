namespace Jarvis.VoiceAgent;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Services;
using Microsoft.AspNetCore.Http;

public sealed class WhisperBackendAdapter : IWhisperBackend
{
    private readonly WhisperService whisperService;

    public WhisperBackendAdapter(WhisperService whisperService)
    {
        this.whisperService = whisperService;
    }

    public async Task<ToolResult> TranscribeAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.Request.Parameters.TryGetValue("Audio", out var audioValue)
            || audioValue is not IFormFile audio)
        {
            return new ToolResult
            {
                ToolName = "WhisperTool",
                Succeeded = false,
                ErrorMessage = "Audio file is required."
            };
        }

        var result = await whisperService.TranscribeAsync(audio, cancellationToken);
        return new ToolResult
        {
            ToolName = "WhisperTool",
            Succeeded = result.Succeeded,
            Output = result.Transcript,
            ErrorMessage = result.Succeeded ? string.Empty : result.Message,
            Value = result
        };
    }
}
