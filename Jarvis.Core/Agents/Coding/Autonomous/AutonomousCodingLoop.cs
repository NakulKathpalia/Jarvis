namespace Jarvis.Core.Agents.Coding.Autonomous;

using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Build;
using Jarvis.Core.Agents.Coding.Context.Intelligent;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Patch;
using Jarvis.Core.Agents.Coding.Review;
using Jarvis.Core.Agents.Coding.Review.Quality;

/// <summary>
/// Runs the bounded autonomous coding loop.
/// </summary>
public sealed class AutonomousCodingLoop
{
    private readonly ICodingModelClient modelClient;
    private readonly CodingSuggestionParser suggestionParser;
    private readonly CodeReviewEngine reviewEngine;
    private readonly PatchEngine patchEngine;
    private readonly BuildEngine buildEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutonomousCodingLoop"/> class.
    /// </summary>
    public AutonomousCodingLoop(ICodingModelClient modelClient)
        : this(modelClient, new CodingSuggestionParser(), new CodeReviewEngine(), new PatchEngine(), new BuildEngine())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutonomousCodingLoop"/> class.
    /// </summary>
    public AutonomousCodingLoop(
        ICodingModelClient modelClient,
        CodingSuggestionParser suggestionParser,
        CodeReviewEngine reviewEngine,
        PatchEngine patchEngine,
        BuildEngine buildEngine)
    {
        this.modelClient = modelClient;
        this.suggestionParser = suggestionParser;
        this.reviewEngine = reviewEngine;
        this.patchEngine = patchEngine;
        this.buildEngine = buildEngine;
    }

    /// <summary>
    /// Runs autonomous iterations.
    /// </summary>
    public async Task<AutonomousCodingResult> RunAsync(
        AutonomousCodingRequest request,
        IntelligentContextResult contextResult,
        CancellationToken cancellationToken = default)
    {
        var result = new AutonomousCodingResult { ContextResult = contextResult };
        var iterations = Math.Clamp(request.MaxIterations <= 0 ? 3 : request.MaxIterations, 1, 10);
        BuildResult? lastBuild = null;
        for (var index = 1; index <= iterations; index++)
        {
            var iteration = await RunIterationAsync(request, contextResult, index, lastBuild, cancellationToken);
            result.Iterations.Add(iteration);
            result.FilesChanged |= iteration.FilesChanged;

            if (!iteration.ModelResponse.Succeeded || request.Mode == CodingExecutionMode.PreviewOnly)
            {
                break;
            }

            if (iteration.BuildResult is null || iteration.BuildResult.Succeeded)
            {
                break;
            }

            lastBuild = iteration.BuildResult;
        }

        result.Succeeded = result.Iterations.Count > 0 && result.Iterations.Last().ModelResponse.Succeeded;
        result.FinalReport = BuildReport(result);
        return result;
    }

    private async Task<CodingIterationResult> RunIterationAsync(
        AutonomousCodingRequest request,
        IntelligentContextResult contextResult,
        int number,
        BuildResult? buildResult,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(request.UserRequest, contextResult, buildResult);
        var modelRequest = new CodingModelRequest
        {
            ModelName = request.ModelName,
            Prompt = prompt
        };
        foreach (var file in contextResult.SelectedFiles.Select(file => file.Path))
        {
            modelRequest.ContextFilePaths.Add(file);
        }

        foreach (var symbol in contextResult.SelectedSymbols.Select(symbol => symbol.Name))
        {
            modelRequest.ContextSymbols.Add(symbol);
        }

        var modelResponse = await modelClient.GenerateAsync(modelRequest, cancellationToken);
        var suggestion = suggestionParser.Parse(modelResponse.Text);
        var preview = new CodingChangePreview
        {
            PatchText = suggestion.PatchText,
            RequiresApproval = true,
            Summary = new CodingChangeSummary
            {
                FilesAffected = suggestion.FilesAffected,
                SuggestedChanges = suggestion.SuggestedChanges,
                SafetyWarnings = suggestion.SafetyWarnings
            }
        };
        var review = reviewEngine.Review(new CodeReviewRequest
        {
            UserRequest = request.UserRequest,
            Suggestion = suggestion,
            ChangePreview = preview
        });
        var iteration = new CodingIterationResult
        {
            Iteration = new CodingIteration { Number = number, Purpose = number == 1 ? "Initial patch proposal" : "Build repair proposal" },
            ModelResponse = modelResponse,
            Suggestion = suggestion,
            ChangePreview = preview,
            ReviewResult = review
        };

        if (CanApply(request, review))
        {
            iteration.PatchResult = patchEngine.Execute(request.ApprovedPatchRequest!);
            iteration.FilesChanged = iteration.PatchResult.Succeeded && !iteration.PatchResult.DryRun;
            iteration.BuildResult = await buildEngine.BuildAsync(new BuildRequest { RepositoryPath = request.RepositoryPath }, cancellationToken);
        }

        return iteration;
    }

    private static bool CanApply(AutonomousCodingRequest request, CodeReviewResult review)
    {
        return request.Mode == CodingExecutionMode.ApplyWithApproval &&
            request.ExplicitApproval &&
            request.ApprovedPatchRequest is not null &&
            !review.BlocksApply;
    }

    private static string BuildPrompt(string userRequest, IntelligentContextResult contextResult, BuildResult? buildResult)
    {
        var prompt = $"User Request: {userRequest}{Environment.NewLine}{Environment.NewLine}{contextResult.ContextText}";
        if (buildResult is not null && !buildResult.Succeeded)
        {
            prompt += $"{Environment.NewLine}Build Errors:{Environment.NewLine}" +
                string.Join(Environment.NewLine, buildResult.Errors.Take(20).Select(error => $"- {error.Code}: {error.Message}"));
        }

        prompt += $"{Environment.NewLine}Respond with sections: Explanation, Files Affected, Suggested Changes, Patch Preview, Safety Warnings.";
        return prompt;
    }

    private static string BuildReport(AutonomousCodingResult result)
    {
        var last = result.Iterations.LastOrDefault();
        return last is null
            ? "No autonomous coding iteration was executed."
            : $"Iterations: {result.Iterations.Count}. Files changed: {result.FilesChanged}. Last model success: {last.ModelResponse.Succeeded}.";
    }
}
