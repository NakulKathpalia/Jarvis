using System.Text.Json;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Data;

public sealed class ChatHistoryService
{
    private readonly string _historyPath;
    private readonly IChatHistoryRepository? _repository;
    private readonly JarvisUserContext _userContext;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly List<ChatMessage> _messages = [];

    public ChatHistoryService(string historyPath, IChatHistoryRepository? repository = null, JarvisUserContext? userContext = null)
    {
        _historyPath = historyPath;
        _repository = repository;
        _userContext = userContext ?? new JarvisUserContext();
    }

    public IReadOnlyCollection<ChatMessage> Messages => _messages;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            var storedMessages = await _repository.GetAsync(_userContext.UserId, cancellationToken);
            _messages.Clear();
            _messages.AddRange(storedMessages);
            return;
        }

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
        message.UserId = _userContext.UserId;
        _messages.Add(message);
        if (_repository is not null)
        {
            await _repository.AddAsync(_userContext.UserId, message, cancellationToken);
        }
        else
        {
            await SaveAsync(cancellationToken);
        }
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
