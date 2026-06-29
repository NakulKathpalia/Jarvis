namespace Jarvis.VoiceAgent;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Voice;

public sealed class VoicePlaybackBackendAdapter : IVoicePlaybackBackend
{
    private readonly TextToSpeechService textToSpeechService;

    public VoicePlaybackBackendAdapter(TextToSpeechService textToSpeechService)
    {
        this.textToSpeechService = textToSpeechService;
    }

    public async Task<ToolResult> PlayAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var operation = context.Request.Parameters.TryGetValue("VoiceOperation", out var value)
            ? value?.ToString() ?? string.Empty
            : string.Empty;

        if (operation.Equals("StopSpeaking", StringComparison.OrdinalIgnoreCase))
        {
            await textToSpeechService.StopAsync(cancellationToken);
            return new ToolResult
            {
                ToolName = "VoicePlaybackTool",
                Succeeded = true,
                Output = "Speech playback stop requested."
            };
        }

        var text = context.Request.Parameters.TryGetValue("Text", out var textValue)
            ? textValue?.ToString() ?? string.Empty
            : context.Request.Input;
        var result = await textToSpeechService.SpeakAsync(text, cancellationToken);
        return new ToolResult
        {
            ToolName = "VoicePlaybackTool",
            Succeeded = result.Succeeded,
            Output = result.AudioUrl,
            ErrorMessage = result.Succeeded ? string.Empty : result.Message,
            Value = result
        };
    }
}
