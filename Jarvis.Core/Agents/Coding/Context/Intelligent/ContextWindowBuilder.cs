namespace Jarvis.Core.Agents.Coding.Context.Intelligent;

using System.Text;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds compact AI-ready context text.
/// </summary>
public sealed class ContextWindowBuilder
{
    /// <summary>
    /// Builds context text from selected files and symbols.
    /// </summary>
    public string Build(
        string userRequest,
        RepositoryContext repositoryContext,
        IReadOnlyList<RelevantFile> files,
        IReadOnlyList<RelevantSymbol> symbols,
        ContextBudget budget)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"User Request: {userRequest}");
        builder.AppendLine($"Repository: {repositoryContext.Summary.RepositoryName}");
        builder.AppendLine($"Languages: {string.Join(", ", repositoryContext.Summary.Languages)}");
        builder.AppendLine($"Projects: {repositoryContext.Statistics.ProjectCount}");
        builder.AppendLine();
        builder.AppendLine("Target Symbols:");
        foreach (var symbol in symbols.Take(budget.MaxSymbols))
        {
            builder.AppendLine($"- {symbol.Kind} {symbol.Name} at {symbol.File}:{symbol.Line}");
        }

        builder.AppendLine();
        builder.AppendLine("Relevant Files:");
        foreach (var file in files.Take(budget.MaxFiles))
        {
            builder.AppendLine($"- {file.Path}:{file.StartLine}-{file.EndLine}");
            foreach (var import in file.ImportStatements.Distinct(StringComparer.OrdinalIgnoreCase).Take(12))
            {
                builder.AppendLine($"  import: {import}");
            }

            var snippet = LimitSnippet(file.SourceSnippet, budget.MaxSnippetLines);
            if (!string.IsNullOrWhiteSpace(snippet))
            {
                builder.AppendLine("  snippet:");
                builder.AppendLine(snippet);
            }
        }

        return builder.ToString();
    }

    private static string LimitSnippet(string snippet, int maxLines)
    {
        return string.Join(
            Environment.NewLine,
            (snippet ?? string.Empty).Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).Take(Math.Max(1, maxLines)));
    }
}
