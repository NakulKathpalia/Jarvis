using System.Diagnostics;

namespace Jarvis.Services;

public sealed class WhisperService
{
    private readonly SettingsService _settingsService;

    public WhisperService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public bool IsConfigured =>
        File.Exists(_settingsService.Current.WhisperExecutablePath)
        && File.Exists(_settingsService.Current.WhisperModelPath);

    public string StatusMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_settingsService.Current.WhisperExecutablePath))
            {
                return "Whisper executable path is not configured.";
            }

            if (!File.Exists(_settingsService.Current.WhisperExecutablePath))
            {
                return "Whisper executable was not found.";
            }

            if (string.IsNullOrWhiteSpace(_settingsService.Current.WhisperModelPath))
            {
                return "Whisper model path is not configured.";
            }

            if (!File.Exists(_settingsService.Current.WhisperModelPath))
            {
                return "Whisper model was not found.";
            }

            return "Whisper is configured.";
        }
    }

    public async Task<WhisperTranscriptionResult> TranscribeAsync(
        IFormFile audio,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return WhisperTranscriptionResult.NotReady(StatusMessage);
        }

        var tempRoot = Path.Combine(Path.GetTempPath(), "jarvis-voice");
        Directory.CreateDirectory(tempRoot);

        var audioPath = Path.Combine(tempRoot, $"{Guid.NewGuid():N}.wav");
        var outputBasePath = Path.Combine(tempRoot, $"{Guid.NewGuid():N}");
        var outputTextPath = $"{outputBasePath}.txt";

        try
        {
            await using (var fileStream = File.Create(audioPath))
            {
                await audio.CopyToAsync(fileStream, cancellationToken);
            }

            var arguments = BuildArguments(audioPath, outputBasePath);
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _settingsService.Current.WhisperExecutablePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();
            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                return WhisperTranscriptionResult.Failed(
                    $"Whisper failed with exit code {process.ExitCode}. {stderr}".Trim());
            }

            var transcript = File.Exists(outputTextPath)
                ? await File.ReadAllTextAsync(outputTextPath, cancellationToken)
                : stdout;

            transcript = transcript.Trim();
            if (string.IsNullOrWhiteSpace(transcript))
            {
                return WhisperTranscriptionResult.Failed("Whisper returned an empty transcript.");
            }

            return WhisperTranscriptionResult.Success(transcript);
        }
        finally
        {
            TryDelete(audioPath);
            TryDelete(outputTextPath);
        }
    }

    private string BuildArguments(string audioPath, string outputBasePath)
    {
        var language = _settingsService.Current.WhisperLanguage;
        var languageArguments = string.IsNullOrWhiteSpace(language) || language.Equals("auto", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : $" -l {Quote(language)}";

        return $"-m {Quote(_settingsService.Current.WhisperModelPath)} -f {Quote(audioPath)} -otxt -of {Quote(outputBasePath)} -nt -np{languageArguments}";
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
            // Temp cleanup should never break the request.
        }
    }
}

public sealed record WhisperTranscriptionResult(
    bool IsReady,
    bool Succeeded,
    string Transcript,
    string Message)
{
    public static WhisperTranscriptionResult Success(string transcript) =>
        new(true, true, transcript, "Transcription complete.");

    public static WhisperTranscriptionResult NotReady(string message) =>
        new(false, false, string.Empty, message);

    public static WhisperTranscriptionResult Failed(string message) =>
        new(true, false, string.Empty, message);
}
