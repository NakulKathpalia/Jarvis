namespace Jarvis.Core.Agents.Coding.Assistant;

using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Autonomous;
using Jarvis.Core.Agents.Coding.Context.Intelligent;
using Jarvis.Core.Agents.Coding.MultiAgent;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Review;
using Jarvis.Core.Agents.Coding.Review.Quality;
using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Represents a local coding assistant result.
/// </summary>
public sealed class CodingAssistantResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the assistant succeeded.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// Gets or sets the prompt preview.
    /// </summary>
    public string PromptPreview { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw model response.
    /// </summary>
    public CodingModelResponse ModelResponse { get; set; } = new();

    /// <summary>
    /// Gets or sets the parsed suggestion.
    /// </summary>
    public CodingSuggestion Suggestion { get; set; } = new();

    /// <summary>
    /// Gets or sets the change preview.
    /// </summary>
    public CodingChangePreview ChangePreview { get; set; } = new();

    /// <summary>
    /// Gets or sets the repository context used.
    /// </summary>
    public RepositoryContext RepositoryContext { get; set; } = new();

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets intelligent context result.
    /// </summary>
    public IntelligentContextResult IntelligentContext { get; set; } = new();

    /// <summary>
    /// Gets or sets autonomous coding result.
    /// </summary>
    public AutonomousCodingResult AutonomousResult { get; set; } = new();

    /// <summary>
    /// Gets or sets review result.
    /// </summary>
    public CodeReviewResult ReviewResult { get; set; } = new();

    /// <summary>
    /// Gets multi-agent results.
    /// </summary>
    public List<CodingAgentResult> MultiAgentResults { get; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether files were changed.
    /// </summary>
    public bool FilesChanged { get; set; }

    /// <summary>
    /// Gets or sets runnable UI result.
    /// </summary>
    public RunnableResult RunnableResult { get; set; } = new();
}
