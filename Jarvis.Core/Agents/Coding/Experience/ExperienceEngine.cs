namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Learns from coding sessions and returns engineering recommendations.
/// </summary>
public sealed class ExperienceEngine
{
    private readonly ExperienceStore store;
    private readonly LearningPolicy policy;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExperienceEngine"/> class.
    /// </summary>
    public ExperienceEngine(ExperienceStore? store = null, LearningPolicy? policy = null)
    {
        this.store = store ?? new ExperienceStore();
        this.policy = policy ?? new LearningPolicy();
    }

    /// <summary>
    /// Records a coding session.
    /// </summary>
    public void Record(ExperienceSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (session.Success || policy.TrackFailures)
        {
            if (!policy.StorePrompts)
            {
                session.Prompt = string.Empty;
                session.CompressedPrompt = string.Empty;
            }

            store.AddSession(session);
        }
    }

    /// <summary>
    /// Queries learned experience for a new request.
    /// </summary>
    public ExperienceResult Query(ExperienceQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var sessions = store.GetSessions(query.Repository);
        var terms = Tokenize(query.UserRequest);
        var result = new ExperienceResult
        {
            RecommendedStrategy = RecommendStrategy(query.UserRequest, sessions)
        };
        result.SimilarSuccessfulSessions.AddRange(sessions
            .Where(session => session.Success)
            .Select(session => new { Session = session, Score = Score(session.UserRequest, terms) })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Session.Timestamp)
            .Take(Math.Max(1, query.MaxResults <= 0 ? policy.MaxSimilarSessions : query.MaxResults))
            .Select(item => item.Session));
        result.CommonFailurePatterns.AddRange(BuildFailurePatterns(sessions));
        AddTop(result.RecommendedFiles, sessions.SelectMany(session => session.SelectedFiles));
        AddTop(result.RecommendedSymbols, sessions.SelectMany(session => session.SelectedSymbols));
        result.ProjectProfile = BuildProjectProfile(query.Repository, sessions);
        return result;
    }

    /// <summary>
    /// Gets aggregate statistics.
    /// </summary>
    public ExperienceStatistics GetStatistics(string repository = "")
    {
        var sessions = store.GetSessions(repository);
        var statistics = new ExperienceStatistics
        {
            TotalSessions = sessions.Count,
            SuccessfulPatches = sessions.Count(session => session.Success && !string.IsNullOrWhiteSpace(session.PatchPreview)),
            FailedPatches = sessions.Count(session => !session.Success),
            SuccessfulBuilds = sessions.Count(session => session.BuildResult?.Succeeded == true)
        };
        AddTop(statistics.FrequentlyModifiedFiles, sessions.SelectMany(session => session.SelectedFiles));
        AddTop(statistics.FrequentlyUsedSymbols, sessions.SelectMany(session => session.SelectedSymbols));
        return statistics;
    }

    private static ProjectCodingProfile BuildProjectProfile(string repository, IReadOnlyList<ExperienceSession> sessions)
    {
        var text = string.Join(Environment.NewLine, sessions.SelectMany(session =>
            new[] { session.Context, session.PatchPreview, session.AppliedPatch }));
        var profile = new ProjectCodingProfile
        {
            Repository = repository,
            UsesFileScopedNamespaces = text.Contains("namespace ", StringComparison.Ordinal) && text.Contains(";", StringComparison.Ordinal),
            UsesAsync = text.Contains("async ", StringComparison.OrdinalIgnoreCase) || text.Contains("Task<", StringComparison.Ordinal),
            UsesDependencyInjection = text.Contains("GetRequiredService", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("AddScoped", StringComparison.OrdinalIgnoreCase),
            UsesMinimalApis = text.Contains("MapGet", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("WebApplication", StringComparison.OrdinalIgnoreCase),
            UsesNullableReferenceTypes = text.Contains("?", StringComparison.Ordinal)
        };
        if (profile.UsesFileScopedNamespaces)
        {
            profile.PreferredStyle.Add("Use file-scoped namespaces.");
        }

        if (profile.UsesAsync)
        {
            profile.PreferredStyle.Add("Prefer async APIs for I/O.");
        }

        if (profile.UsesMinimalApis)
        {
            profile.PreferredStyle.Add("Follow existing minimal API endpoint style.");
        }

        return profile;
    }

    private static IEnumerable<FailurePattern> BuildFailurePatterns(IEnumerable<ExperienceSession> sessions)
    {
        return sessions
            .Where(session => !session.Success && !string.IsNullOrWhiteSpace(session.FailureReason))
            .GroupBy(session => session.FailureReason, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .Take(5)
            .Select(group => new FailurePattern { Reason = group.Key, Count = group.Count() });
    }

    private static string RecommendStrategy(string request, IEnumerable<ExperienceSession> sessions)
    {
        if (request.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
            request.Contains("jwt", StringComparison.OrdinalIgnoreCase))
        {
            return "Inspect authentication, endpoint registration, settings, and middleware before patching.";
        }

        return sessions.Any(session => session.Success)
            ? "Start from similar successful files, keep the patch small, then build."
            : "Build context first, propose a minimal patch, and review before apply.";
    }

    private static void AddTop(ICollection<string> target, IEnumerable<string> values)
    {
        foreach (var value in values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .Select(group => group.Key))
        {
            target.Add(value);
        }
    }

    private static int Score(string text, IReadOnlyCollection<string> terms)
    {
        return terms.Count(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> Tokenize(string text)
    {
        return (text ?? string.Empty)
            .Split([' ', '-', '_', '.', '/', '\\', ':', ';', ','], StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length > 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
