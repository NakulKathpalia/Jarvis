using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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
    public bool CanHandle(IngestionSourceType sourceType) => sourceType == IngestionSourceType.Image;

    public async Task<IngestionExtractionResult> ExtractAsync(IngestionJob job, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(job.StoredPath))
        {
            return Failed("Uploaded image file was not found.");
        }

        if (!await IsTesseractAvailableAsync(cancellationToken))
        {
            return new IngestionExtractionResult(
                false,
                IngestionStatus.OcrRequired,
                string.Empty,
                [],
                "OCR engine not configured.",
                "Tesseract OCR is not installed or not available on PATH.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "tesseract",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(job.StoredPath);
        startInfo.ArgumentList.Add("stdout");
        startInfo.ArgumentList.Add("-l");
        startInfo.ArgumentList.Add("eng");

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

    private static IngestionExtractionResult Failed(string message) =>
        new(false, IngestionStatus.ExtractionFailed, string.Empty, [], message, message);

    private static async Task<bool> IsTesseractAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "tesseract",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = "--version"
            });
            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
