namespace Jarvis.Core.Agents.Coding.Patch;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Builds human-readable patch previews.
/// </summary>
public sealed class PatchPreviewBuilder
{
    /// <summary>
    /// Builds a patch preview.
    /// </summary>
    public PatchPreview Build(PatchPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        return new PatchPreview
        {
            Lines = plan.Operations.Select(Describe).ToList()
        };
    }

    private static string Describe(PatchOperation operation)
    {
        return operation.Type switch
        {
            PatchOperationType.Insert => $"Insert into {operation.TargetPath} at line {operation.StartLine}.",
            PatchOperationType.Replace => $"Replace in {operation.TargetPath}.",
            PatchOperationType.Delete => $"Delete from {operation.TargetPath}.",
            PatchOperationType.Rename => $"Rename {operation.SymbolName} to {operation.NewName} in {operation.TargetPath}.",
            PatchOperationType.Move => $"Move {operation.TargetPath} to {operation.DestinationPath}.",
            PatchOperationType.Extract => $"Extract {operation.TargetPath}:{operation.StartLine}-{operation.EndLine} to {operation.DestinationPath}.",
            PatchOperationType.CreateFile => $"Create file {operation.TargetPath}.",
            PatchOperationType.DeleteFile => $"Delete file {operation.TargetPath}.",
            PatchOperationType.UpdateUsing => $"Update using/import in {operation.TargetPath}.",
            PatchOperationType.UpdateNamespace => $"Update namespace in {operation.TargetPath}.",
            _ => $"{operation.Type} {operation.TargetPath}."
        };
    }
}
