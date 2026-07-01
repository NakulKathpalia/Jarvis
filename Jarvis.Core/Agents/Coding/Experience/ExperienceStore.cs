namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Stores coding experience in memory.
/// </summary>
public sealed class ExperienceStore
{
    private readonly List<ExperienceSession> sessions = [];
    private readonly List<ExperienceEntry> entries = [];
    private readonly object sync = new();

    /// <summary>
    /// Adds a coding session.
    /// </summary>
    public void AddSession(ExperienceSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        lock (sync)
        {
            sessions.Add(session);
            AddEntries(session);
        }
    }

    /// <summary>
    /// Gets a snapshot of stored experience.
    /// </summary>
    public ExperienceSnapshot Snapshot()
    {
        lock (sync)
        {
            var snapshot = new ExperienceSnapshot();
            snapshot.Sessions.AddRange(sessions);
            snapshot.Entries.AddRange(entries);
            return snapshot;
        }
    }

    /// <summary>
    /// Gets sessions for a repository.
    /// </summary>
    public IReadOnlyList<ExperienceSession> GetSessions(string repository)
    {
        lock (sync)
        {
            return sessions
                .Where(session => string.IsNullOrWhiteSpace(repository) ||
                    session.Repository.Equals(repository, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private void AddEntries(ExperienceSession session)
    {
        entries.Add(new ExperienceEntry
        {
            Category = session.Success ? ExperienceCategory.SuccessfulPatch : ExperienceCategory.FailedPatch,
            Text = session.Success ? "Successful coding session." : session.FailureReason,
            Timestamp = session.Timestamp
        });

        foreach (var file in session.SelectedFiles)
        {
            entries.Add(new ExperienceEntry
            {
                Category = session.Success ? ExperienceCategory.SuccessfulPatch : ExperienceCategory.FailedPatch,
                Text = session.UserRequest,
                FilePath = file,
                Timestamp = session.Timestamp
            });
        }

        foreach (var symbol in session.SelectedSymbols)
        {
            entries.Add(new ExperienceEntry
            {
                Category = ExperienceCategory.General,
                Text = session.UserRequest,
                SymbolName = symbol,
                Timestamp = session.Timestamp
            });
        }
    }
}
