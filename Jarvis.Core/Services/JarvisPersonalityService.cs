using Jarvis.Models;

namespace Jarvis.Services;

public sealed class JarvisPersonalityService
{
    private readonly SettingsService _settingsService;

    public JarvisPersonalityService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public string NormalizeUserInput(string input) => JarvisInputNormalizer.StripAssistantPrefix(input);

    public string BuildSystemInstructions(string userInput)
    {
        var settings = _settingsService.Current;
        var style = settings.ResponseStyle == JarvisResponseStyle.Neutral
            ? "Use a clear, neutral assistant style."
            : "Use an original Jarvis-style voice: confident, professional, friendly, efficient, occasionally witty, never childish, never fake-emotional.";
        var language = settings.PreferredLanguage switch
        {
            JarvisPreferredLanguage.English => "Reply in English.",
            JarvisPreferredLanguage.RomanHinglish => "Reply in Roman Hinglish. Do not use Devanagari unless explicitly requested.",
            _ => LooksHindiOrHinglish(userInput)
                ? "The user is using Hindi/Hinglish; prefer Roman Hinglish. Do not use Devanagari unless explicitly requested."
                : "Use English for English or technical input. Use Roman Hinglish only when the user uses Hindi/Hinglish."
        };
        var verbosity = settings.Verbosity switch
        {
            JarvisVerbosity.Detailed => "Use a detailed answer when useful, but stay structured.",
            JarvisVerbosity.Balanced => "Use a balanced answer with enough context to be useful.",
            _ => "Keep responses short by default."
        };

        return string.Join(Environment.NewLine, style, language, verbosity);
    }

    public string FormatCommandResponse(PcCommand command, bool succeeded, string executionMessage)
    {
        if (!succeeded)
        {
            return $"Command failed: {executionMessage}";
        }

        var target = ToDisplayTarget(command);
        return command.Action switch
        {
            PcControlAction.OpenApp => FormatOpened(target),
            PcControlAction.OpenWebsite => FormatOpened(target),
            PcControlAction.OpenFolder => $"Ji sir, {target} folder open kar raha hoon.",
            PcControlAction.OpenFile => $"Ji sir, {target} file open kar raha hoon.",
            PcControlAction.BrowserSearch => $"Ji sir, web search chala raha hoon.",
            PcControlAction.TakeScreenshot => "Ji sir, screenshot le liya.",
            PcControlAction.VolumeUp => "Volume badha diya, sir.",
            PcControlAction.VolumeDown => "Volume kam kar diya, sir.",
            PcControlAction.ToggleMute => "Audio mute toggle kar diya, sir.",
            PcControlAction.Sleep => "Sleep request bhej diya, sir.",
            PcControlAction.Shutdown => "Shutdown schedule kar diya, sir.",
            PcControlAction.Restart => "Restart schedule kar diya, sir.",
            _ => executionMessage
        };
    }

    private static string FormatOpened(string target) =>
        $"Ji sir, {target} open kar raha hoon.";

    private static string ToDisplayTarget(PcCommand command)
    {
        var target = command.Target.Trim();
        if (string.IsNullOrWhiteSpace(target))
        {
            return command.Action.ToString();
        }

        if (target.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || target.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            target = new Uri(target).Host;
        }

        target = target.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? target[4..] : target;
        target = target.EndsWith(".com", StringComparison.OrdinalIgnoreCase) ? target[..^4] : target;
        target = target.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? target[..^4] : target;
        return string.Join(' ', target.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries))
            .Trim()
            .ToLowerInvariant() switch
            {
                "calc" => "Calculator",
                "youtube" => "YouTube",
                "github" => "GitHub",
                var value when value.Length > 0 => char.ToUpperInvariant(value[0]) + value[1..],
                _ => command.Action.ToString()
            };
    }

    private static bool LooksHindiOrHinglish(string input)
    {
        var normalized = input.ToLowerInvariant();
        string[] hints =
        [
            "ji", "haan", "nahi", "kya", "kaise", "mujhe", "tum", "mera", "meri", "mere",
            "kar", "karo", "raha", "rahi", "hai", "hoon", "bata", "yaad"
        ];
        return hints.Any(hint => normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(hint));
    }
}
