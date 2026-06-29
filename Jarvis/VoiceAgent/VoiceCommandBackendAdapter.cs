namespace Jarvis.VoiceAgent;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Services;

public sealed class VoiceCommandBackendAdapter : IVoiceCommandBackend
{
    private readonly VoiceCommandService voiceCommandService;

    public VoiceCommandBackendAdapter(VoiceCommandService voiceCommandService)
    {
        this.voiceCommandService = voiceCommandService;
    }

    public async Task<ToolResult> HandleAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var transcript = context.Request.Parameters.TryGetValue("Transcript", out var value)
            ? value?.ToString() ?? string.Empty
            : context.Request.Input;
        var confirmed = context.Request.Parameters.TryGetValue("Confirmed", out var confirmedValue)
            && confirmedValue is bool isConfirmed
            && isConfirmed;
        var result = await voiceCommandService.TryExecuteAsync(transcript, confirmed, cancellationToken);
        return new ToolResult
        {
            ToolName = "VoiceCommandTool",
            Succeeded = result.Handled || result.RequiresConfirmation,
            Output = result.Message,
            ErrorMessage = result.Handled || result.RequiresConfirmation ? string.Empty : result.Message,
            Value = result
        };
    }
}
