using Microsoft.AspNetCore.Http;

namespace Jarvis.Voice;

public sealed class VoiceActivityDetector
{
    private const long MinimumAudioBytes = 1024;

    public VoiceActivityResult Detect(IFormFile audio)
    {
        if (audio.Length <= 0)
        {
            return VoiceActivityResult.NoSpeech("Audio file is empty.");
        }

        if (audio.Length < MinimumAudioBytes)
        {
            return VoiceActivityResult.NoSpeech("Audio is too short for speech detection.");
        }

            return VoiceActivityResult.Detected("Speech candidate detected.");
    }
}

public sealed record VoiceActivityResult(bool SpeechDetected, string Message)
{
    public static VoiceActivityResult Detected(string message) => new(true, message);

    public static VoiceActivityResult NoSpeech(string message) => new(false, message);
}
