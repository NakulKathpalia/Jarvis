namespace Jarvis.Core.Agents.Coding.Autonomous;

using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Context;
using Jarvis.Core.Agents.Coding.Context.Intelligent;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Parsing;
using Jarvis.Core.Agents.Coding.Planner;
using Jarvis.Core.Agents.Coding.Services;

/// <summary>
/// Coordinates intelligent context, planning, AI suggestions, review, and optional apply.
/// </summary>
public sealed class AutonomousCodingEngine
{
    private readonly ICodingModelClient modelClient;
    private readonly IntelligentContextEngine contextEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutonomousCodingEngine"/> class.
    /// </summary>
    public AutonomousCodingEngine(ICodingModelClient modelClient, IntelligentContextEngine? contextEngine = null)
    {
        this.modelClient = modelClient;
        this.contextEngine = contextEngine ?? new IntelligentContextEngine();
    }

    /// <summary>
    /// Runs autonomous coding.
    /// </summary>
    public async Task<AutonomousCodingResult> RunAsync(
        AutonomousCodingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var repositoryContext = BuildRepositoryContext(request.RepositoryPath, request.UserRequest);
        var context = contextEngine.Build(new IntelligentContextRequest
        {
            UserRequest = request.UserRequest,
            RepositoryContext = repositoryContext,
            Strategy = ContextSelectionStrategy.Balanced,
            CompressionPolicy = ContextCompressionPolicy.BudgetAware
        });
        return await new AutonomousCodingLoop(modelClient).RunAsync(request, context, cancellationToken);
    }

    private static RepositoryContext BuildRepositoryContext(string repositoryPath, string userRequest)
    {
        var reader = new RepositoryReader();
        var readResult = reader.Read(repositoryPath);
        var repositoryIndex = new IndexBuilder().Build(readResult);
        var parserRegistry = new LanguageParserRegistry();
        parserRegistry.Register(new CSharpParser());
        parserRegistry.Register(new TypeScriptParser());
        parserRegistry.Register(new JavaScriptParser());
        var symbolIndex = new SymbolIndexer(parserRegistry).Build(repositoryIndex);
        var knowledge = new RepositoryKnowledgeBuilder().Build(repositoryIndex, symbolIndex);
        var contextPackage = new ContextBuilderV2().Build(new ContextRequest
        {
            RequestText = userRequest,
            RepositoryPath = readResult.RootPath
        }, repositoryIndex, symbolIndex, knowledge);
        var planningResult = new CodingPlanner().Plan(new PlanningRequest
        {
            RequestText = userRequest,
            ContextPackage = contextPackage
        });

        return new RepositoryContextBuilder().Build(repositoryIndex, symbolIndex, knowledge, contextPackage, planningResult);
    }
}
