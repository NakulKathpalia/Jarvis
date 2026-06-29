namespace Jarvis.VoiceAgent;

using Jarvis.Core.Agents.Voice.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Voice;

public sealed class PiperBackendAdapter : IPiperBackend
{
    private readonly TextToSpeechService textToSpeechService;

    public PiperBackendAdapter(TextToSpeechService textToSpeechService)
    {
        this.textToSpeechService = textToSpeechService;
    }

    public async Task<ToolResult> SpeakAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var text = GetText(context);
        var result = await textToSpeechService.SpeakAsync(text, cancellationToken);
        return new ToolResult
        {
            ToolName = "PiperTool",
            Succeeded = result.Succeeded,
            Output = result.AudioUrl,
            ErrorMessage = result.Succeeded ? string.Empty : result.Message,
            Value = result
        };
    }

    private static string GetText(ExecutionContext context)
    {
        return context.Request.Parameters.TryGetValue("Text", out var value)
            ? value?.ToString() ?? string.Empty
            : context.Request.Input;
    }
}
