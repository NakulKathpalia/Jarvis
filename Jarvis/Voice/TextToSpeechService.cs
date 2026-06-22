using System.Diagnostics;
using Jarvis.Services;

namespace Jarvis.Voice;

public sealed class TextToSpeechService
{
    private readonly SettingsService _settingsService;
    private readonly PiperService _piperService;
    private readonly string _audioOutputDirectory;

    public TextToSpeechService(
        SettingsService settingsService,
        PiperService piperService,
        string audioOutputDirectory)
    {
        _settingsService = settingsService;
        _piperService = piperService;
        _audioOutputDirectory = audioOutputDirectory;
        Directory.CreateDirectory(_audioOutputDirectory);
    }

    public bool IsEnabled => _settingsService.Current.EnableVoiceResponses;

    public string Provider => HasEdgeTtsCli ? "Microsoft Edge TTS" : HasWindowsSystemSpeech ? "Windows System Speech" : "Piper";

    public string VoiceName => string.IsNullOrWhiteSpace(_settingsService.Current.VoiceName)
        ? DefaultVoiceName
        : _settingsService.Current.VoiceName.Trim();

    public bool IsAvailable => IsEnabled && (HasEdgeTtsCli || HasWindowsSystemSpeech || _piperService.IsConfigured);

    public string StatusMessage
    {
        get
        {
            if (!IsEnabled)
            {
                return "Voice responses are disabled.";
            }

            if (HasEdgeTtsCli)
            {
                return $"TTS is ready with Microsoft Edge TTS ({VoiceName}).";
            }

            if (HasWindowsSystemSpeech)
            {
                return $"TTS is ready with Windows System Speech ({VoiceName}).";
            }

            return _piperService.IsConfigured
                ? "TTS is ready with Piper fallback."
                : "No TTS provider is available.";
        }
    }

    public async Task<TextToSpeechResult> SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        var spokenText = text.Trim();
        if (string.IsNullOrWhiteSpace(spokenText))
        {
            return TextToSpeechResult.Failed("Text is required.", Provider, VoiceName, spokenText);
        }

        if (!IsEnabled)
        {
            return TextToSpeechResult.NotReady(StatusMessage, Provider, VoiceName, spokenText);
        }

        CleanupOldAudioFiles();

        if (HasEdgeTtsCli)
        {
            var edgeResult = await TrySpeakWithEdgeTtsAsync(spokenText, cancellationToken);
            if (edgeResult.Succeeded)
            {
                return edgeResult;
            }
        }

        if (HasWindowsSystemSpeech)
        {
            var systemResult = await TrySpeakWithSystemSpeechAsync(spokenText, cancellationToken);
            if (systemResult.Succeeded)
            {
                return systemResult;
            }
        }

        if (_piperService.IsConfigured)
        {
            var piperResult = await _piperService.SpeakAsync(spokenText, cancellationToken);
            return new TextToSpeechResult(
                piperResult.IsReady,
                piperResult.Succeeded,
                piperResult.AudioUrl,
                piperResult.Message,
                "Piper",
                "Piper configured voice",
                spokenText,
                piperResult.Succeeded ? EstimateSpeechDurationMs(spokenText) : 0,
                piperResult.Succeeded ? string.Empty : piperResult.Message);
        }

        return TextToSpeechResult.NotReady(StatusMessage, Provider, VoiceName, spokenText);
    }

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private async Task<TextToSpeechResult> TrySpeakWithEdgeTtsAsync(string text, CancellationToken cancellationToken)
    {
        var fileName = $"jarvis-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid():N}.mp3";
        var outputPath = Path.Combine(_audioOutputDirectory, fileName);

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "edge-tts",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        process.StartInfo.ArgumentList.Add("--text");
        process.StartInfo.ArgumentList.Add(text);
        process.StartInfo.ArgumentList.Add("--voice");
        process.StartInfo.ArgumentList.Add(string.IsNullOrWhiteSpace(_settingsService.Current.VoiceName)
            ? "en-US-AriaNeural"
            : VoiceName);
        process.StartInfo.ArgumentList.Add("--rate");
        process.StartInfo.ArgumentList.Add(FormatEdgePercent(_settingsService.Current.SpeechRate - 1.0));
        process.StartInfo.ArgumentList.Add("--volume");
        process.StartInfo.ArgumentList.Add(FormatEdgePercent(_settingsService.Current.SpeechVolume - 1.0));
        process.StartInfo.ArgumentList.Add("--write-media");
        process.StartInfo.ArgumentList.Add(outputPath);

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            return TextToSpeechResult.Failed($"Microsoft Edge TTS failed to start: {ex.Message}", "Microsoft Edge TTS", VoiceName, text);
        }

        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stderr = await stderrTask;

        if (process.ExitCode != 0 || !HasAudioContent(outputPath))
        {
            TryDelete(outputPath);
            return TextToSpeechResult.Failed($"Microsoft Edge TTS failed. {stderr}".Trim(), "Microsoft Edge TTS", VoiceName, text);
        }

        return TextToSpeechResult.Success($"/generated-audio/{fileName}", "Microsoft Edge TTS", VoiceName, text, EstimateSpeechDurationMs(text));
    }

    private async Task<TextToSpeechResult> TrySpeakWithSystemSpeechAsync(string text, CancellationToken cancellationToken)
    {
        var fileName = $"jarvis-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid():N}.wav";
        var outputPath = Path.Combine(_audioOutputDirectory, fileName);
        var textPath = Path.Combine(_audioOutputDirectory, $"{Guid.NewGuid():N}.txt");

        await File.WriteAllTextAsync(textPath, text, cancellationToken);

        var voiceCommand = string.IsNullOrWhiteSpace(_settingsService.Current.VoiceName)
            ? string.Empty
            : $"$s.SelectVoice('{PowerShellQuote(VoiceName)}');";
        var rate = MapSystemSpeechRate(_settingsService.Current.SpeechRate);
        var volume = MapSystemSpeechVolume(_settingsService.Current.SpeechVolume);
        var command =
            "$ErrorActionPreference='Stop';" +
            "Add-Type -AssemblyName System.Speech;" +
            "try{" +
            "$s=New-Object System.Speech.Synthesis.SpeechSynthesizer;" +
            voiceCommand +
            $"$s.Rate={rate};$s.Volume={volume};" +
            $"$s.SetOutputToWaveFile('{PowerShellQuote(outputPath)}');" +
            $"$s.Speak([System.IO.File]::ReadAllText('{PowerShellQuote(textPath)}'));" +
            "$s.Dispose();" +
            "}catch{Write-Error $_;exit 1}";

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            TryDelete(textPath);
            return TextToSpeechResult.Failed($"Windows System Speech failed to start: {ex.Message}", "Windows System Speech", VoiceName, text);
        }

        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stderr = await stderrTask;
        TryDelete(textPath);

        if (process.ExitCode != 0 || !HasAudioContent(outputPath))
        {
            TryDelete(outputPath);
            return TextToSpeechResult.Failed($"Windows System Speech failed. {stderr}".Trim(), "Windows System Speech", VoiceName, text);
        }

        return TextToSpeechResult.Success(
            $"/generated-audio/{fileName}",
            "Windows System Speech",
            VoiceName,
            text,
            ReadWavDurationMs(outputPath));
    }

    private bool HasEdgeTtsCli => FindExecutable("edge-tts") is not null;

    private static bool HasWindowsSystemSpeech => OperatingSystem.IsWindows();

    private static string DefaultVoiceName => OperatingSystem.IsWindows()
        ? "Microsoft Zira Desktop"
        : "Default system voice";

    private void CleanupOldAudioFiles()
    {
        var cutoff = DateTime.UtcNow.AddHours(-6);
        foreach (var file in Directory.EnumerateFiles(_audioOutputDirectory, "jarvis-*.*"))
        {
            try
            {
                if (File.GetCreationTimeUtc(file) < cutoff)
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Generated audio cleanup is best effort.
            }
        }
    }

    private static string? FindExecutable(string name)
    {
        var paths = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        var extensions = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT").Split(';', StringSplitOptions.RemoveEmptyEntries)
            : [string.Empty];

        foreach (var path in paths)
        {
            foreach (var extension in extensions)
            {
                var candidate = Path.Combine(path, name + extension.ToLowerInvariant());
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string FormatEdgePercent(double value)
    {
        var percent = (int)Math.Round(value * 100);
        return percent >= 0 ? $"+{percent}%" : $"{percent}%";
    }

    private static int MapSystemSpeechRate(double rate) =>
        (int)Math.Clamp(Math.Round((rate - 1.0) * 10), -10, 10);

    private static int MapSystemSpeechVolume(double volume) =>
        (int)Math.Clamp(Math.Round(volume * 100), 0, 100);

    private static long EstimateSpeechDurationMs(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return Math.Max(800, (long)(words / 2.4 * 1000));
    }

    private static long ReadWavDurationMs(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            using var reader = new BinaryReader(stream);
            if (new string(reader.ReadChars(4)) != "RIFF")
            {
                return 0;
            }

            stream.Position += 4;
            if (new string(reader.ReadChars(4)) != "WAVE")
            {
                return 0;
            }

            short channels = 1;
            var sampleRate = 16000;
            short bitsPerSample = 16;
            var dataSize = 0;

            while (stream.Position + 8 <= stream.Length)
            {
                var chunkId = new string(reader.ReadChars(4));
                var chunkSize = reader.ReadInt32();
                var chunkStart = stream.Position;

                if (chunkId == "fmt " && chunkSize >= 16)
                {
                    stream.Position += 2;
                    channels = reader.ReadInt16();
                    sampleRate = reader.ReadInt32();
                    stream.Position += 6;
                    bitsPerSample = reader.ReadInt16();
                }
                else if (chunkId == "data")
                {
                    dataSize = chunkSize;
                    break;
                }

                stream.Position = chunkStart + chunkSize + (chunkSize % 2);
            }

            var bytesPerSecond = sampleRate * Math.Max(1, (int)channels) * Math.Max(1, bitsPerSample / 8);
            var durationSeconds = dataSize / (double)bytesPerSecond;
            return (long)Math.Round(durationSeconds * 1000);
        }
        catch
        {
            return 0;
        }
    }

    private static string PowerShellQuote(string value) => value.Replace("'", "''");

    private static bool HasAudioContent(string path)
    {
        return File.Exists(path) && new FileInfo(path).Length > 0;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Ignore cleanup errors.
        }
    }
}

public sealed record TextToSpeechResult(
    bool IsReady,
    bool Succeeded,
    string AudioUrl,
    string Message,
    string Provider,
    string VoiceName,
    string SpokenText,
    long SpeechDurationMs,
    string FailureReason)
{
    public static TextToSpeechResult Success(
        string audioUrl,
        string provider,
        string voiceName,
        string spokenText,
        long speechDurationMs) =>
        new(true, true, audioUrl, "Speech generated.", provider, voiceName, spokenText, speechDurationMs, string.Empty);

    public static TextToSpeechResult NotReady(string message, string provider, string voiceName, string spokenText) =>
        new(false, false, string.Empty, message, provider, voiceName, spokenText, 0, message);

    public static TextToSpeechResult Failed(string message, string provider, string voiceName, string spokenText) =>
        new(true, false, string.Empty, message, provider, voiceName, spokenText, 0, message);
}
