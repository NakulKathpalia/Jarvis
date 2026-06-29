namespace Jarvis.Core.Agents.Coding.Patch;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Restores files captured in patch history.
/// </summary>
public sealed class PatchRollback
{
    /// <summary>
    /// Rolls back a patch execution.
    /// </summary>
    public PatchResult Rollback(PatchHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);

        foreach (var move in history.MoveRollbackMap)
        {
            if (File.Exists(move.Key))
            {
                File.Move(move.Key, move.Value, true);
            }
        }

        foreach (var item in history.OriginalFileContents)
        {
            if (item.Value is null)
            {
                if (File.Exists(item.Key))
                {
                    File.Delete(item.Key);
                }

                continue;
            }

            WriteTextAtomically(item.Key, item.Value);
        }

        return new PatchResult
        {
            Succeeded = true,
            History = history,
            Messages = ["Rollback applied successfully."]
        };
    }

    private static void WriteTextAtomically(string path, string text)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = Path.Combine(directory ?? ".", "." + Path.GetFileName(path) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        File.WriteAllText(tempPath, text);
        File.Move(tempPath, path, true);
    }
}
