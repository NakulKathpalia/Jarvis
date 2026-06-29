namespace Jarvis.Core.Agents.Coding.AI;

using System.Text;
using Jarvis.Core.Agents.Coding.Assistant;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds prompts for local coding suggestions.
/// </summary>
public sealed class CodingPromptBuilder
{
    private readonly CodingPromptTemplate template;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodingPromptBuilder"/> class.
    /// </summary>
    /// <param name="template">The prompt template.</param>
    public CodingPromptBuilder(CodingPromptTemplate? template = null)
    {
        this.template = template ?? new CodingPromptTemplate();
    }

    /// <summary>
    /// Builds a coding prompt.
    /// </summary>
    public string Build(
        string userRequest,
        CodingAssistantMode mode,
        RepositoryContext repositoryContext,
        BuildResult? buildResult = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine(template.Instructions);
        builder.AppendLine($"Mode: {mode}");
        builder.AppendLine($"User Request: {userRequest}");
        builder.AppendLine();
        builder.AppendLine("Repository Context:");
        builder.AppendLine($"Repository: {repositoryContext.Summary.RepositoryName}");
        builder.AppendLine($"Languages: {string.Join(", ", repositoryContext.Summary.Languages)}");
        builder.AppendLine($"Projects: {repositoryContext.Statistics.ProjectCount}");
        builder.AppendLine($"Symbols: {repositoryContext.Knowledge.SymbolCount}");
        builder.AppendLine();
        builder.AppendLine("Relevant Files:");
        foreach (var file in repositoryContext.ContextPackage.RelevantFiles.Take(10))
        {
            builder.AppendLine($"- {file.Path}:{file.StartLine}-{file.EndLine}");
            foreach (var import in file.ImportStatements.Take(8))
            {
                builder.AppendLine($"  import: {import}");
            }

            if (!string.IsNullOrWhiteSpace(file.SourceSnippet))
            {
                builder.AppendLine("  snippet:");
                builder.AppendLine(file.SourceSnippet);
            }
        }

        builder.AppendLine();
        builder.AppendLine("Relevant Symbols:");
        foreach (var symbol in repositoryContext.ContextPackage.RelevantSymbols.Take(20))
        {
            builder.AppendLine($"- {symbol.Kind} {symbol.Name} at {symbol.File}:{symbol.Line}");
        }

        builder.AppendLine();
        builder.AppendLine("Coding Plan:");
        foreach (var step in repositoryContext.PlanningResult.Plan.Steps)
        {
            builder.AppendLine($"- {step.Order}. {step.Title} [{step.Strategy}]");
        }

        if (buildResult is not null)
        {
            builder.AppendLine();
            builder.AppendLine("Build Errors:");
            foreach (var error in buildResult.Errors.Take(20))
            {
                builder.AppendLine($"- {error.File}:{error.Line} {error.Code}: {error.Message}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Respond with sections: Explanation, Files Affected, Suggested Changes, Patch Preview, Safety Warnings.");
        return builder.ToString();
    }
}
