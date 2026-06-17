using Jarvis.Models;

namespace Jarvis.Services;

public sealed class PcCommandParser
{
    public PcCommand Parse(string input)
    {
        var original = input.Trim();
        var normalized = Normalize(original);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return Unknown(original);
        }

        if (TryStripPrefix(normalized, ["search web for ", "browser search ", "web search ", "google "], out var searchQuery))
        {
            return new PcCommand(PcControlAction.BrowserSearch, searchQuery, original);
        }

        if (normalized is "take screenshot" or "screenshot" or "capture screen")
        {
            return new PcCommand(PcControlAction.TakeScreenshot, string.Empty, original);
        }

        if (normalized is "volume up" or "increase volume")
        {
            return new PcCommand(PcControlAction.VolumeUp, string.Empty, original);
        }

        if (normalized is "volume down" or "decrease volume" or "lower volume")
        {
            return new PcCommand(PcControlAction.VolumeDown, string.Empty, original);
        }

        if (normalized is "mute volume" or "unmute volume" or "mute" or "unmute")
        {
            return new PcCommand(PcControlAction.ToggleMute, string.Empty, original);
        }

        if (normalized is "sleep computer" or "put computer to sleep" or "sleep pc")
        {
            return new PcCommand(PcControlAction.Sleep, string.Empty, original);
        }

        if (normalized is "shutdown computer" or "shut down computer" or "shutdown pc" or "shut down pc")
        {
            return new PcCommand(PcControlAction.Shutdown, string.Empty, original);
        }

        if (normalized is "restart computer" or "restart pc" or "reboot computer" or "reboot pc")
        {
            return new PcCommand(PcControlAction.Restart, string.Empty, original);
        }

        if (TryStripPrefix(normalized, ["open website ", "open url ", "go to "], out var website))
        {
            return new PcCommand(PcControlAction.OpenWebsite, website, original);
        }

        if (TryStripPrefix(normalized, ["open folder "], out var folder))
        {
            return new PcCommand(PcControlAction.OpenFolder, folder, original);
        }

        if (TryStripPrefix(normalized, ["open file "], out var file))
        {
            return new PcCommand(PcControlAction.OpenFile, file, original);
        }

        if (TryStripPrefix(normalized, ["open app ", "launch app ", "start app "], out var app))
        {
            return new PcCommand(PcControlAction.OpenApp, app, original);
        }

        if (TryStripPrefix(normalized, ["open ", "launch ", "start "], out var target))
        {
            return LooksLikeWebsite(target)
                ? new PcCommand(PcControlAction.OpenWebsite, target, original)
                : new PcCommand(PcControlAction.OpenApp, target, original);
        }

        return Unknown(original);
    }

    private static PcCommand Unknown(string original)
    {
        return new PcCommand(PcControlAction.Unknown, string.Empty, original);
    }

    private static string Normalize(string value)
    {
        return value.Trim().TrimEnd('.', '!', '?').ToLowerInvariant();
    }

    private static bool LooksLikeWebsite(string target)
    {
        return target.Contains('.', StringComparison.Ordinal)
            || target.Contains("youtube", StringComparison.OrdinalIgnoreCase)
            || target.Contains("google", StringComparison.OrdinalIgnoreCase)
            || target.Contains("github", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryStripPrefix(string input, IReadOnlyCollection<string> prefixes, out string value)
    {
        foreach (var prefix in prefixes)
        {
            if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = input[prefix.Length..].Trim();
                return !string.IsNullOrWhiteSpace(value);
            }
        }

        value = string.Empty;
        return false;
    }
}
