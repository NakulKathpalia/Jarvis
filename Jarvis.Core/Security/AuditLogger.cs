using System.Text.RegularExpressions;

namespace Jarvis.Security;

public sealed class AuditLogger
{
    private static readonly Regex SensitiveValuePattern = new(
        "(password|api[_-]?key|token|secret)\\s*[:=]\\s*\\S+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly string _auditLogPath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public AuditLogger(string auditLogPath)
    {
        _auditLogPath = auditLogPath;
        Directory.CreateDirectory(Path.GetDirectoryName(_auditLogPath) ?? ".");
    }

    public async Task LogAsync(
        SecurityRequest request,
        SecurityAuditResult result,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var line = string.Join(" | ",
        [
            DateTimeOffset.UtcNow.ToString("O"),
            $"source={Mask(request.Source)}",
            $"command={Mask(request.Command)}",
            $"intent={Mask(request.Intent)}",
            $"target={Mask(request.Target)}",
            $"risk={request.RiskLevel}",
            $"result={result.ToString().ToLowerInvariant()}",
            $"reason={Mask(reason)}"
        ]);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(_auditLogPath, line + Environment.NewLine, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static string Mask(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return SensitiveValuePattern.Replace(value, "$1=***");
    }
}
