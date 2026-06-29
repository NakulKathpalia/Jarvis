namespace Jarvis.Core.Agents.Coding.Tools;

using Jarvis.Core.Agents.Coding.Context;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Parsing;
using Jarvis.Core.Agents.Coding.Planner;
using Jarvis.Core.Agents.Coding.Services;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Reads repository structure and returns factual repository context.
/// </summary>
public sealed class RepositoryTool : ToolBase
{
    private readonly RepositoryReader repositoryReader;
    private readonly IndexBuilder indexBuilder;
    private readonly RepositoryContextBuilder contextBuilder;
    private readonly SymbolIndexer symbolIndexer;
    private readonly RepositoryKnowledgeBuilder knowledgeBuilder;
    private readonly ContextBuilderV2 contextBuilderV2;
    private readonly CodingPlanner codingPlanner;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryTool"/> class.
    /// </summary>
    public RepositoryTool()
        : this(
            new RepositoryReader(),
            new IndexBuilder(),
            new RepositoryContextBuilder(),
            new SymbolIndexer(CreateDefaultParserRegistry()),
            new RepositoryKnowledgeBuilder(),
            new ContextBuilderV2(),
            new CodingPlanner())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryTool"/> class.
    /// </summary>
    /// <param name="repositoryReader">The repository reader.</param>
    /// <param name="indexBuilder">The index builder.</param>
    /// <param name="contextBuilder">The context builder.</param>
    /// <param name="symbolIndexer">The symbol indexer.</param>
    /// <param name="knowledgeBuilder">The repository knowledge builder.</param>
    /// <param name="contextBuilderV2">The context builder.</param>
    /// <param name="codingPlanner">The coding planner.</param>
    public RepositoryTool(
        RepositoryReader repositoryReader,
        IndexBuilder indexBuilder,
        RepositoryContextBuilder contextBuilder,
        SymbolIndexer symbolIndexer,
        RepositoryKnowledgeBuilder knowledgeBuilder,
        ContextBuilderV2 contextBuilderV2,
        CodingPlanner codingPlanner)
        : base(new ToolDescriptor
        {
            Name = "RepositoryTool",
            DisplayName = "Repository Tool",
            Description = "Reads repository structure without modifying files."
        })
    {
        this.repositoryReader = repositoryReader;
        this.indexBuilder = indexBuilder;
        this.contextBuilder = contextBuilder;
        this.symbolIndexer = symbolIndexer;
        this.knowledgeBuilder = knowledgeBuilder;
        this.contextBuilderV2 = contextBuilderV2;
        this.codingPlanner = codingPlanner;
    }

    /// <inheritdoc />
    protected override Task<ToolResult> ExecuteCoreAsync(
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repositoryPath = ResolveRepositoryPath(context);
        var requestText = ResolveRequestText(context, repositoryPath);
        var readResult = repositoryReader.Read(repositoryPath);
        var index = indexBuilder.Build(readResult);
        var symbolIndex = symbolIndexer.Build(index);
        var knowledge = knowledgeBuilder.Build(index, symbolIndex);
        var contextRequest = new ContextRequest
        {
            RequestText = requestText,
            RepositoryPath = readResult.RootPath
        };
        var contextPackage = contextBuilderV2.Build(contextRequest, index, symbolIndex, knowledge);
        var planningResult = codingPlanner.Plan(new PlanningRequest
        {
            RequestText = requestText,
            ContextPackage = contextPackage
        });
        var repositoryContext = contextBuilder.Build(index, symbolIndex, knowledge, contextPackage, planningResult);
        var output = contextBuilder.Format(repositoryContext);

        return Task.FromResult(new ToolResult
        {
            ToolName = Name,
            Succeeded = true,
            Output = output,
            Value = repositoryContext
        });
    }

    private static LanguageParserRegistry CreateDefaultParserRegistry()
    {
        var registry = new LanguageParserRegistry();
        registry.Register(new CSharpParser());
        registry.Register(new TypeScriptParser());
        registry.Register(new JavaScriptParser());
        return registry;
    }

    private static string ResolveRepositoryPath(ExecutionContext context)
    {
        if (context.Request.Parameters.TryGetValue("RepositoryPath", out var value) &&
            value is string path &&
            !string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return Directory.Exists(context.Request.Input) || File.Exists(context.Request.Input)
            ? context.Request.Input
            : Directory.GetCurrentDirectory();
    }

    private static string ResolveRequestText(ExecutionContext context, string repositoryPath)
    {
        return string.Equals(context.Request.Input, repositoryPath, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : context.Request.Input;
    }
}
