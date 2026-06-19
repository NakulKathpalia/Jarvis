using Jarvis.Services;

namespace Jarvis.Commands;

public sealed class FileCommand : ICommand
{
    private readonly FileIndexService _fileIndexService;

    public FileCommand(FileIndexService fileIndexService)
    {
        _fileIndexService = fileIndexService;
    }

    public string Name => "file";
    public string Description => "Index and search local files.";
    public string Usage => "/file [index|find <query>]";

    public async Task ExecuteAsync(string arguments, CancellationToken cancellationToken = default)
    {
        var parts = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var action = parts.Length > 0 ? parts[0].ToLowerInvariant() : string.Empty;
        var query = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        if (action == "index")
        {
            var count = await _fileIndexService.RebuildAsync(cancellationToken);
            Console.WriteLine($"Indexed {count} files.");
            return;
        }

        if (action == "find" && !string.IsNullOrWhiteSpace(query))
        {
            foreach (var file in _fileIndexService.Search(query))
            {
                Console.WriteLine(file);
            }
            return;
        }

        Console.WriteLine($"Usage: {Usage}");
    }
}
