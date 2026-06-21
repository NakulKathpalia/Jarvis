using System.Text.RegularExpressions;

namespace Jarvis.Security;

public sealed class InputValidator
{
    private static readonly string[] BlockedTerms =
    [
        "password theft",
        "steal password",
        "token theft",
        "steal token",
        "cookie theft",
        "browser credential",
        "keylogger",
        "malware",
        "spyware",
        "ransomware",
        "disable antivirus",
        "firewall bypass",
        "unauthorized access",
        "privilege escalation",
        "dump credentials"
    ];

    private static readonly string[] SensitivePathTerms =
    [
        "system32",
        "syswow64",
        "program files",
        "program files (x86)",
        ".ssh",
        "id_rsa",
        "known_hosts",
        "appdata\\local\\google\\chrome\\user data",
        "appdata\\roaming\\mozilla",
        ".env",
        ".git",
        "secrets.json",
        ".pem",
        ".key",
        ".pfx",
        ".cer",
        ".crt"
    ];

    private static readonly Regex SecretPattern = new(
        "(password|api[_-]?key|token|secret)\\s*[:=]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public InputValidationResult Validate(SecurityRequest request)
    {
        var command = SanitizeWhitespace(request.Command);
        var target = SanitizeWhitespace(request.Target);
        var combined = $"{command} {target}";

        if (ContainsBlockedTerm(combined))
        {
            return InputValidationResult.Invalid(command, target, "Input matches a blocked security abuse pattern.");
        }

        if (SecretPattern.IsMatch(combined))
        {
            return InputValidationResult.Invalid(command, target, "Input appears to contain a secret value.");
        }

        if (ContainsShellInjection(combined))
        {
            return InputValidationResult.Invalid(command, target, "Input contains shell metacharacters or command chaining.");
        }

        if (ContainsPathTraversal(target))
        {
            return InputValidationResult.Invalid(command, target, "Target contains path traversal.");
        }

        if (TouchesSensitivePath(target))
        {
            return InputValidationResult.Invalid(command, target, "Target points to a protected or sensitive location.");
        }

        if (request.Intent.Contains("Website", StringComparison.OrdinalIgnoreCase)
            && !LooksLikeSafeUrlOrHost(target))
        {
            return InputValidationResult.Invalid(command, target, "Website target is not a valid HTTP(S) URL or host.");
        }

        return InputValidationResult.Valid(command, target);
    }

    private static string SanitizeWhitespace(string value)
    {
        return string.Join(' ', (value ?? string.Empty)
            .ReplaceLineEndings(" ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool ContainsBlockedTerm(string input)
    {
        return BlockedTerms.Any(term => input.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsShellInjection(string input)
    {
        return input.Contains("&&", StringComparison.Ordinal)
            || input.Contains("||", StringComparison.Ordinal)
            || input.Contains("$(", StringComparison.Ordinal)
            || input.Contains('`')
            || input.Contains(';')
            || input.Contains('|')
            || input.Contains('>');
    }

    private static bool ContainsPathTraversal(string target)
    {
        return target.Contains("..\\", StringComparison.Ordinal)
            || target.Contains("../", StringComparison.Ordinal)
            || target.Equals("..", StringComparison.Ordinal);
    }

    private static bool TouchesSensitivePath(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        var normalized = target.Replace('/', '\\').ToLowerInvariant();
        return SensitivePathTerms.Any(term => normalized.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static bool LooksLikeSafeUrlOrHost(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        if (Uri.TryCreate(target, UriKind.Absolute, out var uri))
        {
            return uri.Scheme is "http" or "https" && !string.IsNullOrWhiteSpace(uri.Host);
        }

        return target.All(character =>
                char.IsLetterOrDigit(character)
                || character is '.' or '-' or '_' or '/')
            && !target.Contains('\\', StringComparison.Ordinal);
    }
}
