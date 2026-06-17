using System.Diagnostics;

namespace Jarvis.Services;

public sealed class PiperService
{
    private readonly SettingsService _settingsService;
    private readonly string _audioOutputDirectory;

    public PiperService(SettingsService settingsService, string audioOutputDirectory)
    {
        _settingsService = settingsService;
        _audioOutputDirectory = audioOutputDirectory;
        Directory.CreateDirectory(_audioOutputDirectory);
    }

    public bool IsConfigured =>
        File.Exists(_settingsService.Current.PiperExecutablePath)
        && File.Exists(_settingsService.Current.PiperModelPath);

    public string StatusMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_settingsService.Current.PiperExecutablePath))
            {
                return "Piper executable path is not configured.";
            }

            if (!File.Exists(_settingsService.Current.PiperExecutablePath))
            {
                return "Piper executable was not found.";
            }

            if (string.IsNullOrWhiteSpace(_settingsService.Current.PiperModelPath))
            {
                return "Piper model path is not configured.";
            }

            if (!File.Exists(_settingsService.Current.PiperModelPath))
            {
                return "Piper model was not found.";
            }

            return "Piper is configured.";
        }
    }

    public async Task<PiperSpeechResult> SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return PiperSpeechResult.NotReady(StatusMessage);
        }

        CleanupOldAudioFiles();

        var fileName = $"jarvis-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid():N}.wav";
        var outputPath = Path.Combine(_audioOutputDirectory, fileName);

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _settingsService.Current.PiperExecutablePath,
            Arguments = $"-m {Quote(_settingsService.Current.PiperModelPath)} -f {Quote(outputPath)}",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        process.Start();
        await process.StandardInput.WriteAsync(text);
        await process.StandardInput.FlushAsync(cancellationToken);
        process.StandardInput.Close();

        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            TryDelete(outputPath);
            return PiperSpeechResult.Failed($"Piper failed with exit code {process.ExitCode}. {stderr}".Trim());
        }

        if (!File.Exists(outputPath))
        {
            return PiperSpeechResult.Failed("Piper did not create an audio file.");
        }

        return PiperSpeechResult.Success($"/generated-audio/{fileName}");
    }

    private void CleanupOldAudioFiles()
    {
        var cutoff = DateTime.UtcNow.AddHours(-6);
        foreach (var file in Directory.EnumerateFiles(_audioOutputDirectory, "*.wav"))
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

    private static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

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

public sealed record PiperSpeechResult(
    bool IsReady,
    bool Succeeded,
    string AudioUrl,
    string Message)
{
    public static PiperSpeechResult Success(string audioUrl) =>
        new(true, true, audioUrl, "Speech generated.");

    public static PiperSpeechResult NotReady(string message) =>
        new(false, false, string.Empty, message);

    public static PiperSpeechResult Failed(string message) =>
        new(true, false, string.Empty, message);
}
