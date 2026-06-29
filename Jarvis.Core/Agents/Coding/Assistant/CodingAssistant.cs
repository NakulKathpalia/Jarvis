namespace Jarvis.Core.Agents.Coding.Assistant;

using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Autonomous;
using Jarvis.Core.Agents.Coding.Context;
using Jarvis.Core.Agents.Coding.Context.Intelligent;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.MultiAgent;
using Jarvis.Core.Agents.Coding.Parsing;
using Jarvis.Core.Agents.Coding.Planner;
using Jarvis.Core.Agents.Coding.Review;
using Jarvis.Core.Agents.Coding.Review.Quality;
using Jarvis.Core.Agents.Coding.Services;

/// <summary>
/// Provides local Ollama-backed coding suggestions without applying changes.
/// </summary>
public sealed class CodingAssistant
{
    private readonly ICodingModelClient modelClient;
    private readonly CodingPromptBuilder promptBuilder;
    private readonly CodingSuggestionParser suggestionParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodingAssistant"/> class.
    /// </summary>
    public CodingAssistant(ICodingModelClient modelClient)
        : this(modelClient, new CodingPromptBuilder(), new CodingSuggestionParser())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CodingAssistant"/> class.
    /// </summary>
    public CodingAssistant(
        ICodingModelClient modelClient,
        CodingPromptBuilder promptBuilder,
        CodingSuggestionParser suggestionParser)
    {
        this.modelClient = modelClient;
        this.promptBuilder = promptBuilder;
        this.suggestionParser = suggestionParser;
    }

    /// <summary>
    /// Runs the local coding assistant.
    /// </summary>
    public async Task<CodingAssistantResult> RunAsync(
        CodingAssistantRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var repositoryContext = BuildRepositoryContext(request);
        if (request.Mode is CodingAssistantMode.AutonomousPreview or CodingAssistantMode.AutonomousApplyWithApproval)
        {
            return await RunAutonomousAsync(request, repositoryContext, cancellationToken);
        }

        if (request.Mode == CodingAssistantMode.MultiAgentPreview)
        {
            return await RunMultiAgentAsync(request, cancellationToken);
        }

        var prompt = promptBuilder.Build(request.UserRequest, request.Mode, repositoryContext, request.BuildResult);
        var modelRequest = new CodingModelRequest
        {
            ModelName = request.ModelName,
            Prompt = prompt
        };
        foreach (var file in repositoryContext.ContextPackage.RelevantFiles.Select(file => file.Path))
        {
            modelRequest.ContextFilePaths.Add(file);
        }

        foreach (var symbol in repositoryContext.ContextPackage.RelevantSymbols.Select(symbol => symbol.Name))
        {
            modelRequest.ContextSymbols.Add(symbol);
        }

        var modelResponse = await modelClient.GenerateAsync(modelRequest, cancellationToken);

        if (!modelResponse.Succeeded)
        {
            return new CodingAssistantResult
            {
                Succeeded = false,
                PromptPreview = PreviewPrompt(prompt),
                RepositoryContext = repositoryContext,
                ModelResponse = modelResponse,
                ErrorMessage = modelResponse.ErrorMessage
            };
        }

        var suggestion = suggestionParser.Parse(modelResponse.Text);
        var changePreview = new CodingChangePreview
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
        var review = new CodeReviewEngine().Review(new CodeReviewRequest
        {
            UserRequest = request.UserRequest,
            Suggestion = suggestion,
            ChangePreview = changePreview
        });
        return new CodingAssistantResult
        {
            Succeeded = true,
            PromptPreview = PreviewPrompt(prompt),
            RepositoryContext = repositoryContext,
            ModelResponse = modelResponse,
            Suggestion = suggestion,
            ChangePreview = changePreview,
            ReviewResult = review
        };
    }

    private async Task<CodingAssistantResult> RunAutonomousAsync(
        CodingAssistantRequest request,
        RepositoryContext repositoryContext,
        CancellationToken cancellationToken)
    {
        var context = new IntelligentContextEngine().Build(new IntelligentContextRequest
        {
            UserRequest = request.UserRequest,
            RepositoryContext = repositoryContext
        });
        var autonomous = await new AutonomousCodingLoop(modelClient).RunAsync(new AutonomousCodingRequest
        {
            RepositoryPath = request.RepositoryPath,
            UserRequest = request.UserRequest,
            ModelName = request.ModelName,
            ExplicitApproval = request.ExplicitApproval,
            ApprovedPatchRequest = request.ApprovedPatchRequest,
            Mode = request.Mode == CodingAssistantMode.AutonomousApplyWithApproval
                ? CodingExecutionMode.ApplyWithApproval
                : CodingExecutionMode.PreviewOnly
        }, context, cancellationToken);
        var last = autonomous.Iterations.LastOrDefault();
        return new CodingAssistantResult
        {
            Succeeded = autonomous.Succeeded,
            RepositoryContext = repositoryContext,
            IntelligentContext = context,
            AutonomousResult = autonomous,
            ModelResponse = last?.ModelResponse ?? new CodingModelResponse(),
            Suggestion = last?.Suggestion ?? new CodingSuggestion(),
            ChangePreview = last?.ChangePreview ?? new CodingChangePreview(),
            ReviewResult = last?.ReviewResult ?? new CodeReviewResult(),
            FilesChanged = autonomous.FilesChanged,
            ErrorMessage = autonomous.Succeeded ? string.Empty : last?.ModelResponse.ErrorMessage ?? autonomous.FinalReport
        };
    }

    private async Task<CodingAssistantResult> RunMultiAgentAsync(
        CodingAssistantRequest request,
        CancellationToken cancellationToken)
    {
        var results = await new CodingOrchestrator(modelClient).RunPreviewAsync(
            request.RepositoryPath,
            request.UserRequest,
            request.ModelName,
            cancellationToken);
        var autonomous = results.Select(result => result.AutonomousResult).FirstOrDefault(result => result is not null);
        var last = autonomous?.Iterations.LastOrDefault();
        var output = new CodingAssistantResult
        {
            Succeeded = results.All(result => result.Succeeded || result.Role == CodingAgentRole.Tester),
            AutonomousResult = autonomous ?? new AutonomousCodingResult(),
            IntelligentContext = autonomous?.ContextResult ?? new IntelligentContextResult(),
            ModelResponse = last?.ModelResponse ?? new CodingModelResponse(),
            Suggestion = last?.Suggestion ?? new CodingSuggestion(),
            ChangePreview = last?.ChangePreview ?? new CodingChangePreview(),
            ReviewResult = last?.ReviewResult ?? new CodeReviewResult(),
            FilesChanged = false
        };
        output.MultiAgentResults.AddRange(results);
        return output;
    }

    private static RepositoryContext BuildRepositoryContext(CodingAssistantRequest request)
    {
        var reader = new RepositoryReader();
        var readResult = reader.Read(request.RepositoryPath);
        var repositoryIndex = new IndexBuilder().Build(readResult);
        var parserRegistry = new LanguageParserRegistry();
        parserRegistry.Register(new CSharpParser());
        parserRegistry.Register(new TypeScriptParser());
        parserRegistry.Register(new JavaScriptParser());
        var symbolIndex = new SymbolIndexer(parserRegistry).Build(repositoryIndex);
        var knowledge = new RepositoryKnowledgeBuilder().Build(repositoryIndex, symbolIndex);
        var contextPackage = new ContextBuilderV2().Build(new ContextRequest
        {
            RequestText = request.UserRequest,
            RepositoryPath = readResult.RootPath
        }, repositoryIndex, symbolIndex, knowledge);
        var planningResult = new CodingPlanner().Plan(new PlanningRequest
        {
            RequestText = request.UserRequest,
            ContextPackage = contextPackage
        });

        return new RepositoryContextBuilder().Build(
            repositoryIndex,
            symbolIndex,
            knowledge,
            contextPackage,
            planningResult);
    }

    private static string PreviewPrompt(string prompt)
    {
        const int maxLength = 4000;
        return prompt.Length <= maxLength ? prompt : prompt[..maxLength] + Environment.NewLine + "...";
    }
}
