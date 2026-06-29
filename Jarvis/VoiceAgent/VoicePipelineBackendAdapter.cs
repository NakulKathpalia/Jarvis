namespace Jarvis.VoiceAgent;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Models;
using Jarvis.Services;
using Microsoft.AspNetCore.Http;

public sealed class VoicePipelineBackendAdapter : IVoicePipelineBackend
{
    private readonly VoicePipelineService voicePipelineService;

    public VoicePipelineBackendAdapter(VoicePipelineService voicePipelineService)
    {
        this.voicePipelineService = voicePipelineService;
    }

    public async Task<ToolResult> ProcessAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var operation = GetString(context, "VoiceOperation");
        return operation.Equals("Confirm", StringComparison.OrdinalIgnoreCase)
            ? await ConfirmAsync(context, cancellationToken)
            : await ProcessVoiceAsync(context, cancellationToken);
    }

    private async Task<ToolResult> ProcessVoiceAsync(
        ExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (!context.Request.Parameters.TryGetValue("Audio", out var audioValue)
            || audioValue is not IFormFile audio)
        {
            return Failed("Audio file is required.");
        }

        var requireWakeWord = context.Request.Parameters.TryGetValue("RequireWakeWord", out var value)
            && value is bool required
            && required;
        var result = await voicePipelineService.ProcessAsync(audio, requireWakeWord, cancellationToken);
        return FromPipelineResult(result);
    }

    private async Task<ToolResult> ConfirmAsync(
        ExecutionContext context,
        CancellationToken cancellationToken)
    {
        var confirmationId = GetString(context, "ConfirmationId");
        if (string.IsNullOrWhiteSpace(confirmationId))
        {
            return Failed("Confirmation id is required.");
        }

        var result = await voicePipelineService.ConfirmAsync(confirmationId, cancellationToken);
        return FromPipelineResult(result);
    }

    private static ToolResult FromPipelineResult(VoicePipelineResult result)
    {
        return new ToolResult
        {
            ToolName = "VoicePipelineTool",
            Succeeded = result.Success,
            Output = result.Message,
            ErrorMessage = result.Success ? string.Empty : result.Message,
            Value = result
        };
    }

    private static ToolResult Failed(string message)
    {
        return new ToolResult
        {
            ToolName = "VoicePipelineTool",
            Succeeded = false,
            ErrorMessage = message
        };
    }

    private static string GetString(ExecutionContext context, string key)
    {
        return context.Request.Parameters.TryGetValue(key, out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;
    }
}
