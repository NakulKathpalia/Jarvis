namespace Jarvis.Voice;

public sealed class SpeechService
{
    public Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[TTS placeholder] {text}");
        return Task.CompletedTask;
    }
}
