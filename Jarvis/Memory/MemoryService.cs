using System.Text.Json;
using Jarvis.Models;

namespace Jarvis.Memory;

public sealed class MemoryService
{
    private readonly string _memoryPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly List<MemoryItem> _items = [];

    public MemoryService(string memoryPath)
    {
        _memoryPath = memoryPath;
    }

    public IReadOnlyCollection<MemoryItem> Items => _items;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_memoryPath) ?? ".");
        if (!File.Exists(_memoryPath))
        {
            await SaveAsync(cancellationToken);
            return;
        }

        var json = await File.ReadAllTextAsync(_memoryPath, cancellationToken);
        var items = JsonSerializer.Deserialize<List<MemoryItem>>(json, _jsonOptions) ?? [];

        _items.Clear();
        _items.AddRange(items.Select(NormalizeItem).OrderByDescending(item => item.UpdatedAtUtc));
    }

    public Task<MemoryItem> AddAsync(
        string text,
        string category = "General",
        IEnumerable<string>? tags = null,
        int importance = 3,
        CancellationToken cancellationToken = default)
    {
        var item = NormalizeItem(new MemoryItem
        {
            Id = Guid.NewGuid().ToString(),
            Text = text,
            Category = category,
            Tags = tags?.ToList() ?? new List<string>(),
            Importance = importance,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        _items.Insert(0, item);
        return SaveAndReturnAsync(item, cancellationToken);
    }

    public IReadOnlyList<MemoryItem> Search(string query, string? category = null, string? tag = null, int? minImportance = null)
    {
        var normalizedQuery = NormalizeQuery(query);
        var normalizedCategory = NormalizeQuery(category);
        var normalizedTag = NormalizeQuery(tag);

        return _items
            .Where(item =>
            {
                if (minImportance.HasValue && item.Importance < minImportance.Value)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(normalizedCategory) &&
                    !item.Category.Contains(normalizedCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(normalizedTag) &&
                    !item.Tags.Any(itemTag => itemTag.Contains(normalizedTag, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(normalizedQuery))
                {
                    return true;
                }

                return item.Text.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.Category.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.Tags.Any(itemTag => itemTag.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase));
            })
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToList();
    }

    public async Task<MemoryItem?> UpdateAsync(
        string id,
        string text,
        string category,
        IEnumerable<string>? tags,
        int importance,
        CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        var existing = _items[index];
        existing.Text = text;
        existing.Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
        existing.Tags = NormalizeTags(tags);
        existing.Importance = NormalizeImportance(importance);
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _items.RemoveAt(index);
        _items.Insert(0, existing);

        await SaveAsync(cancellationToken);
        return existing;
    }

    public async Task<MemoryItem?> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        var removed = _items[index];
        _items.RemoveAt(index);
        await SaveAsync(cancellationToken);
        return removed;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _items.Clear();
        await SaveAsync(cancellationToken);
    }

    private async Task<MemoryItem> SaveAndReturnAsync(MemoryItem item, CancellationToken cancellationToken)
    {
        await SaveAsync(cancellationToken);
        return item;
    }

    private Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_items, _jsonOptions);
        return File.WriteAllTextAsync(_memoryPath, json, cancellationToken);
    }

    private static MemoryItem NormalizeItem(MemoryItem item)
    {
        item.Text = item.Text?.Trim() ?? string.Empty;
        item.Category = string.IsNullOrWhiteSpace(item.Category) ? "General" : item.Category.Trim();
        item.Tags = NormalizeTags(item.Tags);
        item.Importance = NormalizeImportance(item.Importance);
        item.CreatedAtUtc = item.CreatedAtUtc == default ? DateTime.UtcNow : item.CreatedAtUtc;
        item.UpdatedAtUtc = item.UpdatedAtUtc == default ? item.CreatedAtUtc : item.UpdatedAtUtc;
        return item;
    }

    private static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        return (tags ?? Enumerable.Empty<string>())
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int NormalizeImportance(int importance)
    {
        return Math.Clamp(importance, 1, 5);
    }

    private static string NormalizeQuery(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
