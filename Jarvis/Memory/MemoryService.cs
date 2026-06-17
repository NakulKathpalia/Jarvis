using System.Text.Json;
using Jarvis.Models;

namespace Jarvis.Memory;

public sealed class MemoryService
{
    private readonly string _memoryPath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
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
        _items.AddRange(items.OrderByDescending(item => item.CreatedAtUtc));
    }

    public async Task AddAsync(string text, string category = "General", CancellationToken cancellationToken = default)
    {
        _items.Insert(0, new MemoryItem
        {
            Id = Guid.NewGuid().ToString(),
            Text = text,
            Category = category,
            CreatedAtUtc = DateTime.UtcNow
        });

        await SaveAsync(cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _items.Clear();
        await SaveAsync(cancellationToken);
    }

    private Task SaveAsync(CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(_items, _jsonOptions);
        return File.WriteAllTextAsync(_memoryPath, json, cancellationToken);
    }
}
