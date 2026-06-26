using System.Text.Json;
using System.Text.Json.Serialization;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Knowledge;

public sealed class KnowledgeService
{
    private readonly string _storagePath;
    private readonly IKnowledgeRepository? _repository;
    private readonly JarvisUserContext _userContext;
    private readonly KnowledgeSearchService _searchService;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly List<KnowledgeItem> _items = [];

    public KnowledgeService(
        string storagePath,
        IKnowledgeRepository? repository = null,
        JarvisUserContext? userContext = null,
        KnowledgeSearchService? searchService = null)
    {
        _storagePath = storagePath;
        _repository = repository;
        _userContext = userContext ?? new JarvisUserContext();
        _searchService = searchService ?? new KnowledgeSearchService();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public IReadOnlyCollection<KnowledgeItem> Items => _items;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            var stored = await _repository.GetForUserAsync(_userContext.UserId, cancellationToken);
            _items.Clear();
            _items.AddRange(stored.Select(Normalize).OrderByDescending(item => item.UpdatedAtUtc));
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_storagePath) ?? ".");
        if (!File.Exists(_storagePath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_storagePath, cancellationToken);
        var items = JsonSerializer.Deserialize<List<KnowledgeItem>>(json, _jsonOptions) ?? [];
        _items.Clear();
        _items.AddRange(items.Select(Normalize).OrderByDescending(item => item.UpdatedAtUtc));
    }

    public async Task<KnowledgeItem> AddAsync(
        string title,
        string content,
        KnowledgeCategory category = KnowledgeCategory.General,
        string source = "Manual",
        string sourceFile = "",
        string sourceType = "",
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var item = Normalize(new KnowledgeItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _userContext.UserId,
            Title = string.IsNullOrWhiteSpace(title) ? "Untitled knowledge" : title.Trim(),
            Content = content,
            Category = category,
            Source = string.IsNullOrWhiteSpace(source) ? "Manual" : source.Trim(),
            SourceFile = sourceFile.Trim(),
            SourceType = sourceType.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

        _items.Insert(0, item);
        await SaveAsync(cancellationToken);
        return item;
    }

    public IReadOnlyList<KnowledgeItem> Search(string? query, KnowledgeCategory? category = null, string? sourceType = null, int limit = 50) =>
        Search(query, category, sourceType, null, null, limit);

    public IReadOnlyList<KnowledgeItem> Search(
        string? query,
        KnowledgeCategory? category,
        string? sourceType,
        DateTime? importedAfterUtc,
        DateTime? importedBeforeUtc,
        int limit = 50)
    {
        var results = _searchService.Search(_items, query, category, sourceType, importedAfterUtc, importedBeforeUtc, limit);
        var now = DateTime.UtcNow;
        foreach (var item in results)
        {
            item.AccessCount++;
            item.LastAccessedAtUtc = now;
        }

        _ = SaveAsync(CancellationToken.None);
        return results;
    }

    public object GetStats() => new
    {
        total = _items.Count,
        categories = _items.GroupBy(item => item.Category)
            .OrderByDescending(group => group.Count())
            .Select(group => new { category = group.Key.ToString(), count = group.Count() })
            .ToList(),
        sources = _items.GroupBy(item => string.IsNullOrWhiteSpace(item.SourceType) ? "Manual" : item.SourceType)
            .OrderByDescending(group => group.Count())
            .Select(group => new { sourceType = group.Key, count = group.Count() })
            .ToList(),
        recent = _items.OrderByDescending(item => item.CreatedAtUtc).Take(8).ToList(),
        frequentlyAccessed = _items.OrderByDescending(item => item.AccessCount).ThenByDescending(item => item.LastAccessedAtUtc).Take(8).ToList()
    };

    public async Task<KnowledgeItem?> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        var removed = _items[index];
        _items.RemoveAt(index);
        if (_repository is not null)
        {
            await _repository.DeleteAsync(_userContext.UserId, id, cancellationToken);
        }
        else
        {
            await SaveAsync(cancellationToken);
        }

        return removed;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_repository is not null)
        {
            await Task.WhenAll(_items.Select(item => _repository.UpsertAsync(_userContext.UserId, item, cancellationToken)));
            return;
        }

        var json = JsonSerializer.Serialize(_items, _jsonOptions);
        await File.WriteAllTextAsync(_storagePath, json, cancellationToken);
    }

    private static KnowledgeItem Normalize(KnowledgeItem item)
    {
        item.UserId = string.IsNullOrWhiteSpace(item.UserId) ? JarvisUserContext.DefaultOwnerUserId : item.UserId;
        item.Title = string.IsNullOrWhiteSpace(item.Title) ? "Untitled knowledge" : item.Title.Trim();
        item.Content = item.Content?.Trim() ?? string.Empty;
        item.Source = string.IsNullOrWhiteSpace(item.Source) ? "Manual" : item.Source.Trim();
        item.SourceFile = Path.GetFileName(item.SourceFile ?? string.Empty);
        item.SourceType = item.SourceType?.Trim() ?? string.Empty;
        item.CharacterCount = item.Content.Length;
        item.WordCount = item.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
        item.CreatedAtUtc = item.CreatedAtUtc == default ? DateTime.UtcNow : item.CreatedAtUtc;
        item.UpdatedAtUtc = item.UpdatedAtUtc == default ? item.CreatedAtUtc : item.UpdatedAtUtc;
        return item;
    }
}
