namespace Jarvis.Models;

public sealed class AppSettings
{
    public const int DefaultOllamaContextLength = 8192;

    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "llama3.2:3b";
    public string PreferredCodingModel { get; set; } = "qwen2.5-coder:14b";
    public int OllamaContextLength { get; set; } = DefaultOllamaContextLength;
    public string SystemPrompt { get; set; } = "You are Jarvis, a local-first, privacy-focused personal AI assistant.";
    public int MaxHistoryMessages { get; set; } = 20;
    public bool MemoryRetrievalEnabled { get; set; } = true;
    public int MaxRetrievedMemories { get; set; } = 5;
    public bool UseTemporaryContext { get; set; } = true;
    public bool UseSuggestedMemories { get; set; } = true;
    public string FileIndexRoot { get; set; } = ".";
    public VoiceMode VoiceMode { get; set; } = VoiceMode.PushToTalk;
    public bool AutoExecuteCommands { get; set; } = true;
    public string VoiceLanguage { get; set; } = "en";
    public bool NoiseSuppression { get; set; } = true;
    public string WhisperExecutablePath { get; set; } = string.Empty;
    public string WhisperModelPath { get; set; } = string.Empty;
    public string WhisperLanguage { get; set; } = "auto";
    public string PiperExecutablePath { get; set; } = string.Empty;
    public string PiperModelPath { get; set; } = string.Empty;
    public string TesseractExecutablePath { get; set; } = string.Empty;
    public string TesseractLanguage { get; set; } = "eng+hin+san";
    public bool EnableVoiceResponses { get; set; } = true;
    public string VoiceName { get; set; } = string.Empty;
    public double SpeechRate { get; set; } = 1.0;
    public double SpeechVolume { get; set; } = 1.0;
    public bool AutoSpeakAssistantReplies { get; set; } = true;
    public bool AutoSpeakResponses { get; set; } = true;
    public JarvisResponseStyle ResponseStyle { get; set; } = JarvisResponseStyle.Jarvis;
    public JarvisPreferredLanguage PreferredLanguage { get; set; } = JarvisPreferredLanguage.Auto;
    public JarvisVerbosity Verbosity { get; set; } = JarvisVerbosity.Short;
    public bool WakeWordEnabled { get; set; }
    public string WakeWordPhrase { get; set; } = "jarvis";
    public string WakeWordDetectorPath { get; set; } = string.Empty;
    public string WakeWordModelPath { get; set; } = string.Empty;
}

public enum JarvisResponseStyle
{
    Jarvis,
    Neutral
}

public enum JarvisPreferredLanguage
{
    Auto,
    English,
    RomanHinglish
}

public enum JarvisVerbosity
{
    Short,
    Balanced,
    Detailed
}
