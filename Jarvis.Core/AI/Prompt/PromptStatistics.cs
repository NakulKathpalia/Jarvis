namespace Jarvis.Core.AI.Prompt;

/// <summary>
/// Contains prompt size statistics.
/// </summary>
public sealed class PromptStatistics
{
    /// <summary>
    /// Gets or sets the prompt character count.
    /// </summary>
    public int CharacterCount { get; set; }

    /// <summary>
    /// Gets or sets the approximate token count.
    /// </summary>
    public int ApproximateTokens { get; set; }

    /// <summary>
    /// Creates statistics for prompt text.
    /// </summary>
    public static PromptStatistics FromText(string text)
    {
        var length = text?.Length ?? 0;
        return new PromptStatistics
        {
            CharacterCount = length,
            ApproximateTokens = Math.Max(1, length / 4)
        };
    }
}
