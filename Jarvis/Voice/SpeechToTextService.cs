using System.Diagnostics;
using Jarvis.Voice.Models;
using Microsoft.AspNetCore.Http;

namespace Jarvis.Voice;

public sealed class SpeechToTextService
{
    private readonly VoiceSettingsService _settingsService;
    private const string GpuDevice = "gpu";
    private const string CpuDevice = "cpu";

    public SpeechToTextService(VoiceSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public bool IsConfigured => _settingsService.IsConfigured;

    public string StatusMessage => _settingsService.StatusMessage;

    public string EngineName => _settingsService.EngineName;

    public string PreferredDevice => _settingsService.PreferredDevice;

    public string FallbackDevice => _settingsService.FallbackDevice;

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

        var audioPath = Path.Combine(tempRoot, $"{Guid.NewGuid():N}{ResolveAudioExtension(audio)}");
        var outputDirectory = Path.Combine(tempRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(outputDirectory);

        try
        {
            await using (var fileStream = File.Create(audioPath))
            {
                await audio.CopyToAsync(fileStream, cancellationToken);
            }

            var gpuResult = await RunSpeechToTextAsync(audioPath, outputDirectory, _settingsService.PreferredDevice, cancellationToken);
            if (gpuResult.Succeeded)
            {
                return gpuResult;
            }

            var cpuResult = await RunSpeechToTextAsync(audioPath, outputDirectory, CpuDevice, cancellationToken);
            return cpuResult.Succeeded
                ? cpuResult
                : SpeechToTextResult.Failed($"{gpuResult.Message} CPU fallback also failed: {cpuResult.Message}", CpuDevice);
        }
        finally
        {
            TryDelete(audioPath);
            TryDeleteDirectory(outputDirectory);
        }
    }

    private async Task<SpeechToTextResult> RunSpeechToTextAsync(
        string audioPath,
        string outputDirectory,
        string device,
        CancellationToken cancellationToken)
    {
        var arguments = _settingsService.IsWhisperCppConfigured
            ? BuildWhisperCppArguments(audioPath, outputDirectory, device)
            : BuildFasterWhisperArguments(audioPath, outputDirectory, device);
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = _settingsService.Current.WhisperExecutablePath,
            Arguments = arguments,
            WorkingDirectory = Path.GetDirectoryName(_settingsService.Current.WhisperExecutablePath) ?? string.Empty,
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
            return SpeechToTextResult.Failed($"{_settingsService.EngineName} failed to start: {ex.Message}", device);
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            return SpeechToTextResult.Failed(
                $"{_settingsService.EngineName} {device} failed with exit code {process.ExitCode}. {stderr}".Trim(),
                device);
        }

        var transcript = await ReadTranscriptAsync(outputDirectory, stdout, cancellationToken);
        return string.IsNullOrWhiteSpace(transcript)
            ? SpeechToTextResult.Failed($"{_settingsService.EngineName} returned an empty transcript.", device)
            : SpeechToTextResult.Success(transcript.Trim(), device);
    }

    private string BuildFasterWhisperArguments(string audioPath, string outputDirectory, string device)
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

    private string BuildWhisperCppArguments(string audioPath, string outputDirectory, string device)
    {
        var outputBase = Path.Combine(outputDirectory, "transcript");
        var language = _settingsService.Language;
        var languageArguments = language.Equals("auto", StringComparison.OrdinalIgnoreCase)
            ? " -l auto"
            : $" -l {Quote(language)}";
        var gpuArguments = device.Equals(CpuDevice, StringComparison.OrdinalIgnoreCase)
            ? " -ng"
            : string.Empty;

        return $"-m {Quote(_settingsService.Current.WhisperModelPath)} -f {Quote(audioPath)} -otxt -of {Quote(outputBase)} -nt -np{languageArguments}{gpuArguments}";
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

    private static string ResolveAudioExtension(IFormFile audio)
    {
        var fileExtension = Path.GetExtension(audio.FileName);
        if (!string.IsNullOrWhiteSpace(fileExtension))
        {
            return fileExtension;
        }

        return audio.ContentType.Equals("audio/wav", StringComparison.OrdinalIgnoreCase)
            || audio.ContentType.Equals("audio/x-wav", StringComparison.OrdinalIgnoreCase)
            ? ".wav"
            : ".webm";
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
