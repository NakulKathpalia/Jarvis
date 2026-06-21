namespace Jarvis.Voice.Models;

public sealed record SpeechToTextResult(
    bool IsReady,
    bool Succeeded,
    string Transcript,
    string Message,
    string Device = "cpu")
{
    public static SpeechToTextResult Success(string transcript, string device) =>
        new(true, true, transcript, "Transcription complete.", device);

    public static SpeechToTextResult NotReady(string message) =>
        new(false, false, string.Empty, message);

    public static SpeechToTextResult Failed(string message, string device = "cpu") =>
        new(true, false, string.Empty, message, device);
}
