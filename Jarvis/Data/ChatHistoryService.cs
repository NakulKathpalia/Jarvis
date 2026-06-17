using System.Text.Json;
using Jarvis.Models;

namespace Jarvis.Data;

public sealed class ChatHistoryService
{
    private readonly string _historyPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly List<ChatMessage> _messages = [];

    public ChatHistoryService(string historyPath)
    {
        _historyPath = historyPath;
    }

    public IReadOnlyCollection<ChatMessage> Messages => _messages;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_historyPath) ?? ".");
        if (!File.Exists(_historyPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_historyPath, cancellationToken);
        var messages = JsonSerializer.Deserialize<List<ChatMessage>>(json, _jsonOptions) ?? [];
        _messages.Clear();
        _messages.AddRange(messages);
    }

    public async Task AddAsync(ChatMessage message, CancellationToken cancellationToken = default)
    {
        _messages.Add(message);
        await SaveAsync(cancellationToken);
    }

    public IEnumerable<ChatMessage> GetRecent(int count)
    {
        return _messages.Count <= count
            ? _messages
            : _messages.Skip(_messages.Count - count);
    }

    private Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_messages, _jsonOptions);
        return File.WriteAllTextAsync(_historyPath, json, cancellationToken);
    }
}
