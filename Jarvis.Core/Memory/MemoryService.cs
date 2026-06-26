using System.Text.Json;
using Jarvis.Models;
using Jarvis.Repositories;
using Jarvis.Users;

namespace Jarvis.Memory;

public sealed class MemoryService
{
    private readonly string _memoryPath;
    private readonly IMemoryRepository? _repository;
    private readonly JarvisUserContext _userContext;
    private readonly MemoryClassificationService _classificationService;
    private readonly MemoryScoringService _scoringService;
    private readonly MemorySearchService _searchService;
    private readonly MemoryReviewService _reviewService;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly List<MemoryItem> _items = [];

    public MemoryService(
        string memoryPath,
        IMemoryRepository? repository = null,
        JarvisUserContext? userContext = null,
        MemoryClassificationService? classificationService = null,
        MemoryScoringService? scoringService = null,
        MemorySearchService? searchService = null,
        MemoryReviewService? reviewService = null)
    {
        _memoryPath = memoryPath;
        _repository = repository;
        _userContext = userContext ?? new JarvisUserContext();
        _classificationService = classificationService ?? new MemoryClassificationService();
        _scoringService = scoringService ?? new MemoryScoringService();
        _searchService = searchService ?? new MemorySearchService();
        _reviewService = reviewService ?? new MemoryReviewService();
    }

    public IReadOnlyCollection<MemoryItem> Items => _items;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            var storedItems = await _repository.GetForUserAsync(_userContext.UserId, cancellationToken);
            _items.Clear();
            _items.AddRange(storedItems.Select(NormalizeItem).OrderByDescending(item => item.UpdatedAtUtc));
            return;
        }

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
        int confidence = 10,
        string source = "Manual",
        MemoryType memoryType = MemoryType.PermanentMemory,
        MemoryReviewStatus? reviewStatus = null,
        DateTime? expiresAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = _classificationService.NormalizeType(memoryType, source);
        var item = NormalizeItem(new MemoryItem
        {
            Id = Guid.NewGuid().ToString(),
            UserId = _userContext.UserId,
            Text = text,
            Category = _classificationService.NormalizeCategory(category),
            Tags = tags?.ToList() ?? new List<string>(),
            Importance = importance,
            Confidence = confidence,
            Source = string.IsNullOrWhiteSpace(source) ? "Manual" : source.Trim(),
            MemoryType = normalizedType,
            ReviewStatus = reviewStatus ?? _classificationService.DefaultReviewStatus(normalizedType),
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });

        _items.Insert(0, item);
        return SaveAndReturnAsync(item, cancellationToken);
    }

    public IReadOnlyList<MemoryItem> Search(
        string query,
        string? category = null,
        string? tag = null,
        int? minImportance = null,
        int? minConfidence = null,
        MemoryType? memoryType = null,
        MemoryReviewStatus? reviewStatus = null,
        bool includeExpired = false)
    {
        return Search(new MemorySearchCriteria(
            query,
            category,
            tag,
            minImportance,
            minConfidence,
            memoryType,
            reviewStatus,
            includeExpired));
    }

    public IReadOnlyList<MemoryItem> Search(MemorySearchCriteria criteria) =>
        _searchService.Search(_items, criteria);

    public IReadOnlyList<MemoryItem> GetPendingSuggestions() =>
        _items.Where(_reviewService.IsPending)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ToList();

    public async Task<MemoryItem?> UpdateAsync(
        string id,
        string text,
        string category,
        IEnumerable<string>? tags,
        int importance,
        int? confidence = null,
        string? source = null,
        MemoryType? memoryType = null,
        MemoryReviewStatus? reviewStatus = null,
        DateTime? expiresAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var index = _items.FindIndex(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return null;
        }

        var existing = _items[index];
        existing.Text = text;
        existing.Category = _classificationService.NormalizeCategory(category);
        existing.Tags = NormalizeTags(tags);
        existing.Importance = _scoringService.NormalizeImportance(importance);
        existing.Confidence = _scoringService.NormalizeConfidence(confidence ?? existing.Confidence);
        existing.Source = string.IsNullOrWhiteSpace(source) ? existing.Source : source.Trim();
        existing.MemoryType = memoryType ?? existing.MemoryType;
        existing.ReviewStatus = reviewStatus ?? existing.ReviewStatus;
        existing.ExpiresAtUtc = expiresAtUtc;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        _items.RemoveAt(index);
        _items.Insert(0, existing);

        await SaveAsync(cancellationToken);
        return existing;
    }

    public async Task<MemoryItem?> ApproveAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = Find(id);
        if (item is null)
        {
            return null;
        }

        _reviewService.Approve(item);
        await SaveAsync(cancellationToken);
        return item;
    }

    public async Task<MemoryItem?> RejectAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = Find(id);
        if (item is null)
        {
            return null;
        }

        _reviewService.Reject(item);
        await SaveAsync(cancellationToken);
        return item;
    }

    public async Task<IReadOnlyList<MemoryItem>> BulkApproveAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var updated = _items.Where(item => idSet.Contains(item.Id)).ToList();
        foreach (var item in updated)
        {
            _reviewService.Approve(item);
            if (item.MemoryType == MemoryType.SuggestedMemory)
            {
                item.MemoryType = MemoryType.PermanentMemory;
            }
        }

        await SaveAsync(cancellationToken);
        return updated;
    }

    public async Task<IReadOnlyList<MemoryItem>> BulkRejectAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var updated = _items.Where(item => idSet.Contains(item.Id)).ToList();
        foreach (var item in updated)
        {
            _reviewService.Reject(item);
        }

        await SaveAsync(cancellationToken);
        return updated;
    }

    public async Task<int> BulkDeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var removed = _items.RemoveAll(item => idSet.Contains(item.Id));
        await SaveAsync(cancellationToken);
        return removed;
    }

    public async Task<IReadOnlyList<MemoryItem>> BulkUpdateAsync(
        IEnumerable<string> ids,
        string? category = null,
        int? importance = null,
        int? confidence = null,
        bool convertSuggestedToPermanent = false,
        CancellationToken cancellationToken = default)
    {
        var idSet = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var updated = _items.Where(item => idSet.Contains(item.Id)).ToList();
        foreach (var item in updated)
        {
            if (!string.IsNullOrWhiteSpace(category))
            {
                item.Category = _classificationService.NormalizeCategory(category);
            }

            if (importance is not null)
            {
                item.Importance = _scoringService.NormalizeImportance(importance.Value);
            }

            if (confidence is not null)
            {
                item.Confidence = _scoringService.NormalizeConfidence(confidence.Value);
            }

            if (convertSuggestedToPermanent && item.MemoryType == MemoryType.SuggestedMemory)
            {
                item.MemoryType = MemoryType.PermanentMemory;
                item.ReviewStatus = MemoryReviewStatus.Approved;
            }

            item.UpdatedAtUtc = DateTime.UtcNow;
        }

        await SaveAsync(cancellationToken);
        return updated;
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

    private MemoryItem? Find(string id) =>
        _items.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    private Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (_repository is not null)
        {
            return Task.WhenAll(_items.Select(item => _repository.UpsertAsync(_userContext.UserId, item, cancellationToken)));
        }

        var json = JsonSerializer.Serialize(_items, _jsonOptions);
        return File.WriteAllTextAsync(_memoryPath, json, cancellationToken);
    }

    private static MemoryItem NormalizeItem(MemoryItem item)
    {
        item.Text = item.Text?.Trim() ?? string.Empty;
        item.UserId = string.IsNullOrWhiteSpace(item.UserId) ? JarvisUserContext.DefaultOwnerUserId : item.UserId;
        item.Category = MemoryCategories.Normalize(item.Category);
        item.Tags = NormalizeTags(item.Tags);
        item.Importance = Math.Clamp(item.Importance, 1, 10);
        item.Confidence = Math.Clamp(item.Confidence <= 0 ? 10 : item.Confidence, 1, 10);
        item.Source = string.IsNullOrWhiteSpace(item.Source) ? "Manual" : item.Source.Trim();
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

}
