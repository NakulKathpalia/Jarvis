using System.Diagnostics;
using Jarvis.Voice.Models;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Voice;

public sealed class SpeechToTextService
{
    private readonly VoiceSettingsService _settingsService;

    public SpeechToTextService(VoiceSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public bool IsConfigured => _settingsService.IsConfigured;

    public string StatusMessage => _settingsService.StatusMessage;

    public async Task<SpeechToTextResult> TranscribeAsync(
        IFormFile audio,
        CancellationToken cancellationToken = default)
    {
        if (!_settingsService.IsConfigured)
        {
            return SpeechToTextResult.NotReady(_settingsService.StatusMessage);
        }

        var tempRoot = Path.Combine(Path.GetTempPath(), "jarvis-voice");
        Directory.CreateDirectory(tempRoot);

        var audioPath = Path.Combine(tempRoot, $"{Guid.NewGuid():N}.webm");
        var outputDirectory = Path.Combine(tempRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);

        try
        {
            await using (var fileStream = File.Create(audioPath))
            {
                await audio.CopyToAsync(fileStream, cancellationToken);
            }

            var gpuResult = await RunFasterWhisperAsync(audioPath, outputDirectory, "cuda", cancellationToken);
            if (gpuResult.Succeeded)
            {
                return gpuResult;
            }

            var cpuResult = await RunFasterWhisperAsync(audioPath, outputDirectory, "cpu", cancellationToken);
            return cpuResult.Succeeded
                ? cpuResult
                : SpeechToTextResult.Failed($"{gpuResult.Message} CPU fallback also failed: {cpuResult.Message}", "cpu");
        }
        finally
        {
            TryDelete(audioPath);
            TryDeleteDirectory(outputDirectory);
        }
    }

    private async Task<SpeechToTextResult> RunFasterWhisperAsync(
        string audioPath,
        string outputDirectory,
        string device,
        CancellationToken cancellationToken)
    {
        var arguments = BuildArguments(audioPath, outputDirectory, device);
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

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            return SpeechToTextResult.Failed($"Faster-Whisper failed to start: {ex.Message}", device);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            return SpeechToTextResult.Failed(
                $"Faster-Whisper {device} failed with exit code {process.ExitCode}. {stderr}".Trim(),
                device);
        }

        var transcript = await ReadTranscriptAsync(outputDirectory, stdout, cancellationToken);
        return string.IsNullOrWhiteSpace(transcript)
            ? SpeechToTextResult.Failed("Faster-Whisper returned an empty transcript.", device)
            : SpeechToTextResult.Success(transcript.Trim(), device);
    }

    private string BuildArguments(string audioPath, string outputDirectory, string device)
    {
        var language = _settingsService.Language;
        var languageArguments = language.Equals("auto", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : $" --language {Quote(language)}";
        var computeType = device.Equals("cuda", StringComparison.OrdinalIgnoreCase)
            ? "float16"
            : "int8";

        return $"{Quote(audioPath)} --model {Quote(_settingsService.Current.WhisperModelPath)} --output_dir {Quote(outputDirectory)} --output_format txt --device {device} --compute_type {computeType}{languageArguments}";
    }

    private static async Task<string> ReadTranscriptAsync(
        string outputDirectory,
        string stdout,
        CancellationToken cancellationToken)
    {
        var transcriptFile = Directory.EnumerateFiles(outputDirectory, "*.txt", SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        return transcriptFile is null
            ? stdout.Trim()
            : (await File.ReadAllTextAsync(transcriptFile, cancellationToken)).Trim();
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

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Temp cleanup should never break the request.
        }
    }
}
