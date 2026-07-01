namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Detects requests that should produce runnable UI output.
/// </summary>
public sealed class RunnableTaskDetector
{
    private static readonly string[] UiTerms =
    [
        "create a login page",
        "login page",
        "create navbar",
        "navbar",
        "shopping app ui",
        "make react ui",
        "build html",
        "html/css/js",
        "create ui",
        "make ui"
    ];

    /// <summary>
    /// Detects the runnable task type.
    /// </summary>
    public RunnableTaskType Detect(string request)
    {
        var text = request ?? string.Empty;
        if (text.Contains("react", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("vite", StringComparison.OrdinalIgnoreCase))
        {
            return RunnableTaskType.React;
        }

        return UiTerms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase))
            ? RunnableTaskType.Html
            : RunnableTaskType.None;
    }

    /// <summary>
    /// Returns a value indicating whether the request is runnable.
    /// </summary>
    public bool IsRunnableUiRequest(string request)
    {
        return Detect(request) != RunnableTaskType.None;
    }
}
