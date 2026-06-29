namespace Jarvis.Core.Agents.Coding.Build;

using System.Text.RegularExpressions;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Parses compiler output into errors and warnings.
/// </summary>
public sealed class ErrorAnalyzer
{
    private static readonly Regex DotnetMessage = new(
        @"^(?<file>.*)\((?<line>\d+),(?<column>\d+)\):\s(?<level>error|warning)\s(?<code>[A-Z]+\d+):\s(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses compiler errors.
    /// </summary>
    public IReadOnlyList<CompilerError> ParseErrors(string output)
    {
        return Parse(output)
            .Where(message => message.Level.Equals("error", StringComparison.OrdinalIgnoreCase))
            .Select(message => new CompilerError
            {
                File = message.File,
                Line = message.Line,
                Code = message.Code,
                Message = message.Message
            })
            .ToList();
    }

    /// <summary>
    /// Parses compiler warnings.
    /// </summary>
    public IReadOnlyList<CompilerWarning> ParseWarnings(string output)
    {
        return Parse(output)
            .Where(message => message.Level.Equals("warning", StringComparison.OrdinalIgnoreCase))
            .Select(message => new CompilerWarning
            {
                File = message.File,
                Line = message.Line,
                Code = message.Code,
                Message = message.Message
            })
            .ToList();
    }

    private static IEnumerable<ParsedCompilerMessage> Parse(string output)
    {
        foreach (var line in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var match = DotnetMessage.Match(line.Trim());
            if (!match.Success)
            {
                continue;
            }

            yield return new ParsedCompilerMessage(
                match.Groups["file"].Value,
                int.Parse(match.Groups["line"].Value),
                match.Groups["level"].Value,
                match.Groups["code"].Value,
                match.Groups["message"].Value);
        }
    }

    private sealed record ParsedCompilerMessage(
        string File,
        int Line,
        string Level,
        string Code,
        string Message);
}
