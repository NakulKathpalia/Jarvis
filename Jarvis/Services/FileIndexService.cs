namespace Jarvis.Services;

public sealed class FileIndexService
{
    private readonly SettingsService _settingsService;
    private readonly List<string> _indexedFiles = [];

    public FileIndexService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyCollection<string> IndexedFiles => _indexedFiles;

    public Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(_settingsService.Current.FileIndexRoot);
        if (!Directory.Exists(root))
        {
            return Task.FromResult(0);
        }

        _indexedFiles.Clear();
        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                || file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _indexedFiles.Add(file);
        }

        return Task.FromResult(_indexedFiles.Count);
    }

    public IEnumerable<string> Search(string query)
    {
        return _indexedFiles
            .Where(file => Path.GetFileName(file).Contains(query, StringComparison.OrdinalIgnoreCase)
                || file.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(20);
    }
}
