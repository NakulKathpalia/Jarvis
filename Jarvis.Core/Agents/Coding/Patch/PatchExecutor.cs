namespace Jarvis.Core.Agents.Coding.Patch;

using System.Text.RegularExpressions;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Executes deterministic patch plans atomically.
/// </summary>
public sealed class PatchExecutor
{
    /// <summary>
    /// Executes a patch plan.
    /// </summary>
    public PatchResult Execute(PatchPlan plan, PatchPreview preview)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(preview);

        var result = new PatchResult { DryRun = plan.DryRun, Preview = preview, Succeeded = true };
        if (plan.DryRun)
        {
            result.Messages.Add("Dry run completed. No files were modified.");
            return result;
        }

        try
        {
            foreach (var operation in plan.Operations)
            {
                CaptureOriginals(operation, result.History);
                Apply(operation);
                result.History.AppliedOperationIds.Add(operation.OperationId);
            }

            result.Messages.Add("Patch applied successfully.");
            return result;
        }
        catch (Exception ex)
        {
            result.Succeeded = false;
            result.Messages.Add(ex.Message);
            new PatchRollback().Rollback(result.History);
            result.Messages.Add("Rollback completed after patch failure.");
            return result;
        }
    }

    private static void CaptureOriginals(PatchOperation operation, PatchHistory history)
    {
        Capture(operation.TargetPath, history);
        if (!string.IsNullOrWhiteSpace(operation.DestinationPath))
        {
            Capture(operation.DestinationPath, history);
        }

        if (operation.Type == PatchOperationType.Move)
        {
            history.MoveRollbackMap[operation.DestinationPath] = operation.TargetPath;
        }
    }

    private static void Capture(string path, PatchHistory history)
    {
        if (string.IsNullOrWhiteSpace(path) || history.OriginalFileContents.ContainsKey(path))
        {
            return;
        }

        history.OriginalFileContents[path] = File.Exists(path) ? File.ReadAllText(path) : null;
    }

    private static void Apply(PatchOperation operation)
    {
        switch (operation.Type)
        {
            case PatchOperationType.Insert:
                WriteTextAtomically(operation.TargetPath, Insert(ReadText(operation.TargetPath), operation));
                break;
            case PatchOperationType.Replace:
                WriteTextAtomically(operation.TargetPath, Replace(ReadText(operation.TargetPath), operation));
                break;
            case PatchOperationType.Delete:
                WriteTextAtomically(operation.TargetPath, Delete(ReadText(operation.TargetPath), operation));
                break;
            case PatchOperationType.Rename:
                WriteTextAtomically(operation.TargetPath, Rename(ReadText(operation.TargetPath), operation));
                break;
            case PatchOperationType.Move:
                MoveAtomically(operation.TargetPath, operation.DestinationPath);
                break;
            case PatchOperationType.Extract:
                Extract(operation);
                break;
            case PatchOperationType.CreateFile:
                WriteTextAtomically(operation.TargetPath, operation.Text);
                break;
            case PatchOperationType.DeleteFile:
                File.Delete(operation.TargetPath);
                break;
            case PatchOperationType.UpdateUsing:
                WriteTextAtomically(operation.TargetPath, UpdateUsing(ReadText(operation.TargetPath), operation.Text));
                break;
            case PatchOperationType.UpdateNamespace:
                WriteTextAtomically(operation.TargetPath, UpdateNamespace(ReadText(operation.TargetPath), operation.Text));
                break;
        }
    }

    private static string Insert(string text, PatchOperation operation)
    {
        var lines = SplitLines(text);
        var index = operation.StartLine <= 0 ? lines.Count : Math.Min(operation.StartLine - 1, lines.Count);
        lines.Insert(index, operation.Text);
        return string.Join(Environment.NewLine, lines);
    }

    private static string Replace(string text, PatchOperation operation)
    {
        if (!string.IsNullOrEmpty(operation.SearchText))
        {
            return text.Replace(operation.SearchText, operation.ReplaceText, StringComparison.Ordinal);
        }

        return ReplaceLineRange(text, operation, operation.Text);
    }

    private static string Delete(string text, PatchOperation operation)
    {
        if (!string.IsNullOrEmpty(operation.SearchText))
        {
            return text.Replace(operation.SearchText, string.Empty, StringComparison.Ordinal);
        }

        return ReplaceLineRange(text, operation, string.Empty);
    }

    private static string Rename(string text, PatchOperation operation)
    {
        return Regex.Replace(
            text,
            $@"\b{Regex.Escape(operation.SymbolName)}\b",
            operation.NewName);
    }

    private static string ReplaceLineRange(string text, PatchOperation operation, string replacement)
    {
        var lines = SplitLines(text);
        var start = Math.Max(1, operation.StartLine);
        var end = Math.Min(lines.Count, Math.Max(start, operation.EndLine));
        lines.RemoveRange(start - 1, end - start + 1);
        if (!string.IsNullOrEmpty(replacement))
        {
            lines.Insert(start - 1, replacement);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static void Extract(PatchOperation operation)
    {
        var lines = SplitLines(ReadText(operation.TargetPath));
        var start = Math.Max(1, operation.StartLine);
        var end = Math.Min(lines.Count, Math.Max(start, operation.EndLine));
        var extracted = string.Join(Environment.NewLine, lines.Skip(start - 1).Take(end - start + 1));
        WriteTextAtomically(operation.DestinationPath, extracted);
        WriteTextAtomically(operation.TargetPath, ReplaceLineRange(ReadText(operation.TargetPath), operation, string.Empty));
    }

    private static string UpdateUsing(string text, string importStatement)
    {
        if (text.Contains(importStatement, StringComparison.Ordinal))
        {
            return text;
        }

        var lines = SplitLines(text);
        var insertIndex = lines.FindLastIndex(line => line.TrimStart().StartsWith("using ", StringComparison.Ordinal));
        lines.Insert(insertIndex < 0 ? 0 : insertIndex + 1, importStatement);
        return string.Join(Environment.NewLine, lines);
    }

    private static string UpdateNamespace(string text, string namespaceDeclaration)
    {
        return Regex.Replace(text, @"namespace\s+[A-Za-z0-9_.]+", namespaceDeclaration, RegexOptions.Multiline);
    }

    private static string ReadText(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
    }

    private static List<string> SplitLines(string text)
    {
        return text.Replace("\r\n", "\n").Split('\n').ToList();
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

    private static void MoveAtomically(string sourcePath, string destinationPath)
    {
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Move(sourcePath, destinationPath, true);
    }
}
