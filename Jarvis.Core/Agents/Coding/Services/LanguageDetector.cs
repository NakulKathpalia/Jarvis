namespace Jarvis.Core.Agents.Coding.Services;

/// <summary>
/// Detects programming language from file extension.
/// </summary>
public sealed class LanguageDetector
{
    /// <summary>
    /// Detects the language for a file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns>The detected language, or an empty string.</returns>
    public string Detect(string filePath)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".cs" => "C#",
            ".ts" or ".tsx" => "TypeScript",
            ".js" or ".jsx" or ".mjs" or ".cjs" => "JavaScript",
            ".py" => "Python",
            ".rs" => "Rust",
            ".go" => "Go",
            ".java" => "Java",
            ".cpp" or ".cc" or ".cxx" or ".hpp" or ".h" => "C++",
            _ => string.Empty
        };
    }
}
