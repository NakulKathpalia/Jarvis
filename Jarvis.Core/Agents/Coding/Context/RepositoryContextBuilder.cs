namespace Jarvis.Core.Agents.Coding.Context;

using System.Text;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds factual repository context from a repository index.
/// </summary>
public sealed class RepositoryContextBuilder
{
    private const long LargeFileThresholdBytes = 1024 * 1024;

    /// <summary>
    /// Builds repository context.
    /// </summary>
    /// <param name="index">The repository index.</param>
    /// <returns>The repository context.</returns>
    public RepositoryContext Build(RepositoryIndex index)
    {
        return Build(index, new SymbolIndex(), new RepositoryKnowledge());
    }

    /// <summary>
    /// Builds repository context with code intelligence.
    /// </summary>
    /// <param name="index">The repository index.</param>
    /// <param name="symbolIndex">The symbol index.</param>
    /// <param name="knowledge">The repository knowledge.</param>
    /// <returns>The repository context.</returns>
    public RepositoryContext Build(
        RepositoryIndex index,
        SymbolIndex symbolIndex,
        RepositoryKnowledge knowledge)
    {
        return Build(index, symbolIndex, knowledge, new ContextPackage(), new PlanningResult());
    }

    /// <summary>
    /// Builds repository context with request-specific coding context and planning.
    /// </summary>
    public RepositoryContext Build(
        RepositoryIndex index,
        SymbolIndex symbolIndex,
        RepositoryKnowledge knowledge,
        ContextPackage contextPackage,
        PlanningResult planningResult)
    {
        ArgumentNullException.ThrowIfNull(index);

        return new RepositoryContext
        {
            Index = index,
            Summary = BuildSummary(index),
            Statistics = BuildStatistics(index),
            Symbols = symbolIndex,
            Knowledge = knowledge,
            ContextPackage = contextPackage,
            PlanningResult = planningResult,
            RepositoryTree = BuildTree(index)
        };
    }

    /// <summary>
    /// Formats repository context as a plain text report.
    /// </summary>
    /// <param name="context">The repository context.</param>
    /// <returns>The formatted repository report.</returns>
    public string Format(RepositoryContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var builder = new StringBuilder();
        builder.AppendLine($"Repository Name: {context.Summary.RepositoryName}");
        builder.AppendLine($"Projects: {context.Statistics.ProjectCount}");
        builder.AppendLine($"Languages: {JoinOrNone(context.Summary.Languages)}");
        builder.AppendLine($"Configurations: {JoinOrNone(context.Summary.Configurations)}");
        builder.AppendLine($"Folders: {context.Statistics.FolderCount}");
        builder.AppendLine($"Files: {context.Statistics.FileCount}");
        builder.AppendLine($"Large Files: {context.Statistics.LargeFileCount}");
        builder.AppendLine($"Average Project Size: {context.Statistics.AverageProjectSize}");
        builder.AppendLine($"Symbol Count: {context.Knowledge.SymbolCount}");
        builder.AppendLine($"Total Classes: {context.Knowledge.ClassCount}");
        builder.AppendLine($"Total Methods: {context.Knowledge.MethodCount}");
        builder.AppendLine($"Interfaces: {context.Knowledge.InterfaceCount}");
        builder.AppendLine($"Properties: {context.Knowledge.PropertyCount}");
        builder.AppendLine($"Fields: {context.Knowledge.FieldCount}");
        builder.AppendLine($"Average Methods Per Class: {context.Knowledge.AverageMethodsPerClass}");
        builder.AppendLine("Namespaces:");
        AppendLines(builder, context.Knowledge.Namespaces.Take(30));
        builder.AppendLine("Largest Classes:");
        AppendLines(builder, context.Knowledge.LargestClasses);
        builder.AppendLine("Largest Files:");
        AppendLines(builder, context.Knowledge.LargestFiles);
        builder.AppendLine("ContextPackage:");
        builder.AppendLine($"- Relevant Projects: {context.ContextPackage.Statistics.RelevantProjectCount}");
        builder.AppendLine($"- Relevant Files: {context.ContextPackage.Statistics.RelevantFileCount}");
        builder.AppendLine($"- Relevant Symbols: {context.ContextPackage.Statistics.RelevantSymbolCount}");
        builder.AppendLine("Relevant Files:");
        AppendLines(builder, context.ContextPackage.RelevantFiles.Select(file => $"{file.Path}:{file.StartLine}-{file.EndLine}"));
        builder.AppendLine("Relevant Symbols:");
        AppendLines(builder, context.ContextPackage.RelevantSymbols.Select(symbol => $"{symbol.Kind}:{symbol.Name}@{symbol.File}:{symbol.Line}"));
        builder.AppendLine("CodingPlan:");
        AppendLines(builder, context.PlanningResult.Plan.Steps.Select(step => $"{step.Order}. {step.Title} [{step.Strategy}]"));
        builder.AppendLine("Dependency Summary:");
        AppendLines(builder, context.Summary.DependencySummary);
        builder.AppendLine("Repository Tree:");
        AppendLines(builder, context.RepositoryTree.Take(80));
        return builder.ToString().TrimEnd();
    }

    private static RepositorySummary BuildSummary(RepositoryIndex index)
    {
        return new RepositorySummary
        {
            RepositoryName = index.RepositoryName,
            Languages = index.Languages.FileCounts.Keys.OrderBy(language => language, StringComparer.OrdinalIgnoreCase).ToList(),
            Configurations = index.Configurations.Configurations
                .Select(configuration => configuration.ConfigurationType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(configuration => configuration, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            DependencySummary = index.Dependencies.Count == 0
                ? ["No project references detected."]
                : index.Dependencies.Select(dependency => $"{dependency.SourceProject} -> {dependency.ReferencePath}").ToList()
        };
    }

    private static RepositoryStatistics BuildStatistics(RepositoryIndex index)
    {
        var projectCount = index.Projects.Projects.Count;
        return new RepositoryStatistics
        {
            ProjectCount = projectCount,
            FolderCount = index.Folders.Count,
            FileCount = index.Files.Files.Count,
            LargeFileCount = index.Files.Files.Count(file => file.SizeBytes >= LargeFileThresholdBytes),
            AverageProjectSize = projectCount == 0
                ? 0
                : Math.Round((double)index.Files.Files.Count / projectCount, 2)
        };
    }

    private static List<string> BuildTree(RepositoryIndex index)
    {
        return index.Folders
            .Select(folder => folder.RelativePath + "/")
            .Concat(index.Files.Files.Select(file => file.RelativePath))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string JoinOrNone(IEnumerable<string> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? "None" : string.Join(", ", list);
    }

    private static void AppendLines(StringBuilder builder, IEnumerable<string> lines)
    {
        var any = false;
        foreach (var line in lines)
        {
            any = true;
            builder.AppendLine($"- {line}");
        }

        if (!any)
        {
            builder.AppendLine("- None");
        }
    }
}
