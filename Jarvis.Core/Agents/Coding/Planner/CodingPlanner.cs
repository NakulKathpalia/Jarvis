namespace Jarvis.Core.Agents.Coding.Planner;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds read-only coding plans from factual context packages.
/// </summary>
public sealed class CodingPlanner
{
    private readonly PlannerValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodingPlanner"/> class.
    /// </summary>
    /// <param name="validator">The planner validator.</param>
    public CodingPlanner(PlannerValidator? validator = null)
    {
        this.validator = validator ?? new PlannerValidator();
    }

    /// <summary>
    /// Creates a read-only coding plan.
    /// </summary>
    public PlanningResult Plan(PlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = validator.Validate(request).ToList();
        if (messages.Any(message => message.Contains("required", StringComparison.OrdinalIgnoreCase)))
        {
            return new PlanningResult { Succeeded = false, Messages = messages };
        }

        var package = request.ContextPackage;
        var plan = new CodingPlan
        {
            RequestText = request.RequestText,
            Steps =
            [
                CreateStep(1, "Read authentication-related context", PlanningStrategy.Read, package),
                CreateStep(2, "Locate middleware and request pipeline", PlanningStrategy.Locate, package, "middleware", "pipeline"),
                CreateStep(3, "Locate dependency injection registration", PlanningStrategy.Locate, package, "service", "startup", "runtime"),
                CreateStep(4, "Locate settings and configuration", PlanningStrategy.Locate, package, "settings", "configuration"),
                CreateStep(5, "Locate controllers and endpoints", PlanningStrategy.Locate, package, "controller", "endpoint"),
                CreateStep(6, "Prepare candidate patch targets", PlanningStrategy.PreparePatchTargets, package)
            ]
        };

        return new PlanningResult
        {
            Succeeded = true,
            Plan = plan,
            Messages = messages
        };
    }

    private static CodingStep CreateStep(
        int order,
        string title,
        PlanningStrategy strategy,
        ContextPackage package,
        params string[] filters)
    {
        var files = package.RelevantFiles
            .Where(file => MatchesAny(file.Path, filters))
            .Select(file => file.Path)
            .Take(8)
            .ToList();
        var symbols = package.RelevantSymbols
            .Where(symbol => MatchesAny(symbol.Name + " " + symbol.File + " " + symbol.Kind, filters))
            .Select(symbol => $"{symbol.Kind}:{symbol.Name}@{symbol.File}:{symbol.Line}")
            .Take(12)
            .ToList();

        if (files.Count == 0 && filters.Length == 0)
        {
            files = package.RelevantFiles.Select(file => file.Path).Take(8).ToList();
        }

        if (symbols.Count == 0 && filters.Length == 0)
        {
            symbols = package.RelevantSymbols.Select(symbol => $"{symbol.Kind}:{symbol.Name}@{symbol.File}:{symbol.Line}").Take(12).ToList();
        }

        return new CodingStep
        {
            Order = order,
            Title = title,
            Strategy = strategy,
            TargetFiles = files,
            TargetSymbols = symbols
        };
    }

    private static bool MatchesAny(string value, IReadOnlyCollection<string> filters)
    {
        return filters.Count == 0 ||
            filters.Any(filter => value.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }
}
