using System.Text.Json;
using Jarvis.Models;

namespace Jarvis.Data;

public sealed class ChatSessionService
{
    private readonly string _sessionsPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly List<ChatSession> _sessions = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ChatSessionService(string sessionsPath)
    {
        _sessionsPath = sessionsPath;
    }

    public IReadOnlyCollection<ChatSession> Sessions => _sessions;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_sessionsPath) ?? ".");
        if (!File.Exists(_sessionsPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_sessionsPath, cancellationToken);
        var sessions = JsonSerializer.Deserialize<List<ChatSession>>(json, _jsonOptions) ?? [];
        _sessions.Clear();
        _sessions.AddRange(sessions.OrderByDescending(session => session.UpdatedAtUtc));
    }

    public IReadOnlyCollection<ChatSessionSummary> GetSummaries()
    {
        return _sessions
            .OrderByDescending(session => session.UpdatedAtUtc)
            .Select(ToSummary)
            .ToArray();
    }

    public ChatSession? Get(string id)
    {
        return _sessions.FirstOrDefault(session => session.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ChatSession> CreateAsync(string? title = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var session = new ChatSession
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = string.IsNullOrWhiteSpace(title) ? "New chat" : title.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _sessions.Insert(0, session);
            await SaveAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }

        return session;
    }

    public async Task AddMessageAsync(string id, ChatMessage message, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var session = Get(id) ?? throw new InvalidOperationException("Chat session not found.");
            session.Messages.Add(message);
            session.UpdatedAtUtc = DateTime.UtcNow;
            if (session.Title.Equals("New chat", StringComparison.OrdinalIgnoreCase)
                && message.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                session.Title = CreateTitle(message.Content);
            }

            await SaveAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var removed = _sessions.RemoveAll(session => session.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) > 0;
            if (removed)
            {
                await SaveAsync(cancellationToken);
            }

            return removed;
        }
        finally
        {
            _lock.Release();
        }
    }

    private Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_sessions, _jsonOptions);
        return File.WriteAllTextAsync(_sessionsPath, json, cancellationToken);
    }

    private static ChatSessionSummary ToSummary(ChatSession session)
    {
        var preview = session.Messages.LastOrDefault(message => !string.IsNullOrWhiteSpace(message.Content))?.Content
            ?? "Fresh conversation";

        return new ChatSessionSummary(
            session.Id,
            session.Title,
            preview.Length <= 120 ? preview : $"{preview[..117]}...",
            session.CreatedAtUtc,
            session.UpdatedAtUtc,
            session.Messages.Count);
    }

    private static string CreateTitle(string content)
    {
        var normalized = string.Join(' ', content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length <= 42 ? normalized : $"{normalized[..39]}...";
    }
}
