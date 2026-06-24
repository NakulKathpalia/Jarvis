using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Jarvis.Services;

namespace Jarvis.Ingestion;

public interface ITextExtractionService
{
    bool CanHandle(IngestionSourceType sourceType);
    Task<IngestionExtractionResult> ExtractAsync(IngestionJob job, CancellationToken cancellationToken = default);
}

public sealed class PdfTextExtractionService : ITextExtractionService
{
    private static readonly Regex LiteralTextPattern = new(@"\((?:\\.|[^\\)])*\)", RegexOptions.Compiled);

    public bool CanHandle(IngestionSourceType sourceType) => sourceType == IngestionSourceType.Pdf;

    public async Task<IngestionExtractionResult> ExtractAsync(IngestionJob job, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(job.StoredPath))
        {
            return Failed("Uploaded PDF file was not found.");
        }

        var bytes = await File.ReadAllBytesAsync(job.StoredPath, cancellationToken);
        var raw = Encoding.Latin1.GetString(bytes);
        var fragments = LiteralTextPattern.Matches(raw)
            .Select(match => DecodePdfLiteral(match.Value))
            .Select(CleanText)
            .Where(text => IsReadableText(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var text = CleanText(string.Join(Environment.NewLine, fragments));
        if (string.IsNullOrWhiteSpace(text))
        {
            return new IngestionExtractionResult(
                false,
                IngestionStatus.OcrRequired,
                string.Empty,
                [],
                "PDF has no readable embedded text. OCR is required.",
                "PDF has no embedded text.");
        }

        var block = new IngestionTextBlock
        {
            PageNumber = 1,
            Text = text,
            Confidence = 7,
            Status = "EmbeddedText"
        };

        return new IngestionExtractionResult(
            true,
            IngestionStatus.Extracted,
            text,
            [block],
            "Embedded PDF text extracted.");
    }

    private static IngestionExtractionResult Failed(string message) =>
        new(false, IngestionStatus.ExtractionFailed, string.Empty, [], message, message);

    private static string DecodePdfLiteral(string value)
    {
        var content = value.Length >= 2 ? value[1..^1] : value;
        var builder = new StringBuilder(content.Length);
        for (var index = 0; index < content.Length; index++)
        {
            var current = content[index];
            if (current != '\\' || index == content.Length - 1)
            {
                builder.Append(current);
                continue;
            }

            var next = content[++index];
            builder.Append(next switch
            {
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'b' => '\b',
                'f' => '\f',
                '(' => '(',
                ')' => ')',
                '\\' => '\\',
                _ => next
            });
        }

        return builder.ToString();
    }

    private static string CleanText(string text) =>
        Regex.Replace(text.Replace('\0', ' '), @"[ \t]+", " ").Trim();

    private static bool IsReadableText(string text)
    {
        if (text.Length < 3)
        {
            return false;
        }

        var readable = text.Count(character => !char.IsControl(character) && (char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) || char.IsPunctuation(character)));
        return readable >= Math.Max(3, text.Length / 2);
    }
}

public sealed class ImageOcrService : ITextExtractionService
{
    private readonly SettingsService _settingsService;

    public ImageOcrService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public bool CanHandle(IngestionSourceType sourceType) => sourceType == IngestionSourceType.Image;

    public async Task<TesseractOcrStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var executablePath = ResolveTesseractExecutable();
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            return new TesseractOcrStatus(
                false,
                "OCR Not Configured",
                "Tesseract OCR is not installed or not available on PATH.",
                string.Empty,
                EffectiveLanguage,
                false,
                []);
        }

        var languageStatus = await GetInstalledLanguagesAsync(executablePath, cancellationToken);
        if (!string.IsNullOrWhiteSpace(languageStatus.ErrorMessage))
        {
            return new TesseractOcrStatus(
                false,
                "OCR Error",
                languageStatus.ErrorMessage,
                executablePath,
                EffectiveLanguage,
                false,
                languageStatus.Languages);
        }

        var missingLanguages = RequiredLanguages
            .Where(language => !languageStatus.Languages.Contains(language, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missingLanguages.Contains("hin", StringComparer.OrdinalIgnoreCase))
        {
            return new TesseractOcrStatus(
                false,
                "OCR Not Configured",
                "Hindi OCR language pack not installed.",
                executablePath,
                EffectiveLanguage,
                false,
                languageStatus.Languages);
        }

        if (missingLanguages.Count > 0)
        {
            return new TesseractOcrStatus(
                false,
                "OCR Not Configured",
                $"OCR language pack missing: {string.Join(", ", missingLanguages)}.",
                executablePath,
                EffectiveLanguage,
                languageStatus.Languages.Contains("hin", StringComparer.OrdinalIgnoreCase),
                languageStatus.Languages);
        }

        return new TesseractOcrStatus(
            true,
            "OCR Available",
            $"Tesseract OCR is ready for {EffectiveLanguage}.",
            executablePath,
            EffectiveLanguage,
            languageStatus.Languages.Contains("hin", StringComparer.OrdinalIgnoreCase),
            languageStatus.Languages);
    }

    public async Task<IngestionExtractionResult> ExtractAsync(IngestionJob job, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(job.StoredPath))
        {
            return Failed("Uploaded image file was not found.");
        }

        var status = await GetStatusAsync(cancellationToken);
        if (!status.Available)
        {
            return new IngestionExtractionResult(
                false,
                IngestionStatus.OcrRequired,
                string.Empty,
                [],
                status.Status,
                status.Message);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = status.ExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(job.StoredPath);
        startInfo.ArgumentList.Add("stdout");
        startInfo.ArgumentList.Add("-l");
        startInfo.ArgumentList.Add(status.Language);

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return Failed("OCR engine could not be started.");
        }

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var text = output.Trim();
        if (process.ExitCode != 0)
        {
            return Failed(string.IsNullOrWhiteSpace(error) ? "OCR failed." : error.Trim());
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return new IngestionExtractionResult(
                false,
                IngestionStatus.OcrRequired,
                string.Empty,
                [],
                "No readable text detected in image.",
                "OCR completed but returned no text.");
        }

        return new IngestionExtractionResult(
            true,
            IngestionStatus.Extracted,
            text,
            [new IngestionTextBlock { Text = text, Confidence = 5, Status = "OcrText" }],
            "Image OCR completed.");
    }

    private string EffectiveLanguage =>
        string.IsNullOrWhiteSpace(_settingsService.Current.TesseractLanguage)
            ? "eng+hin+san"
            : _settingsService.Current.TesseractLanguage.Trim();

    private IReadOnlyList<string> RequiredLanguages =>
        EffectiveLanguage.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static IngestionExtractionResult Failed(string message) =>
        new(false, IngestionStatus.ExtractionFailed, string.Empty, [], message, message);

    private string ResolveTesseractExecutable()
    {
        var configuredPath = _settingsService.Current.TesseractExecutablePath;
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        string[] commonPaths =
        [
            @"C:\Program Files\Tesseract-OCR\tesseract.exe",
            @"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Tesseract-OCR", "tesseract.exe")
        ];

        var commonPath = commonPaths.FirstOrDefault(File.Exists);
        if (!string.IsNullOrWhiteSpace(commonPath))
        {
            return commonPath;
        }

        var pathValue = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var candidate = Path.Combine(directory, "tesseract.exe");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch
            {
                // Ignore malformed PATH entries.
            }
        }

        return string.Empty;
    }

    private static async Task<TesseractLanguageStatus> GetInstalledLanguagesAsync(string executablePath, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "--list-langs"
            };
            using var process = Process.Start(startInfo);
            if (process is null)
            {
                return new TesseractLanguageStatus([], "OCR engine could not be started.");
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode != 0)
            {
                return new TesseractLanguageStatus(
                    [],
                    string.IsNullOrWhiteSpace(error) ? "Tesseract OCR language check failed." : error.Trim());
            }

            var languages = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(line => !line.StartsWith("List of available languages", StringComparison.OrdinalIgnoreCase))
                .ToList();
            return new TesseractLanguageStatus(languages, string.Empty);
        }
        catch (Exception ex)
        {
            return new TesseractLanguageStatus([], $"Tesseract OCR check failed: {ex.Message}");
        }
    }
}

internal sealed record TesseractLanguageStatus(IReadOnlyList<string> Languages, string ErrorMessage);

public sealed record TesseractOcrStatus(
    bool Available,
    string Status,
    string Message,
    string ExecutablePath,
    string Language,
    bool HindiAvailable,
    IReadOnlyList<string> InstalledLanguages);
