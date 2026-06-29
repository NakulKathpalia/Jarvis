namespace Jarvis.Core.Agents.Coding.Patch;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Validates deterministic patch requests.
/// </summary>
public sealed class PatchValidator
{
    /// <summary>
    /// Validates a patch request.
    /// </summary>
    /// <param name="request">The patch request.</param>
    /// <returns>Validation messages.</returns>
    public IReadOnlyList<string> Validate(PatchRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var messages = new List<string>();
        if (request.Operations.Count == 0)
        {
            messages.Add("Patch request must contain at least one operation.");
        }

        foreach (var operation in request.Operations)
        {
            ValidateOperation(operation, messages);
        }

        return messages;
    }

    private static void ValidateOperation(PatchOperation operation, List<string> messages)
    {
        if (string.IsNullOrWhiteSpace(operation.TargetPath))
        {
            messages.Add($"Operation {operation.OperationId} requires a target path.");
        }

        if ((operation.Type is PatchOperationType.Move or PatchOperationType.Extract) &&
            string.IsNullOrWhiteSpace(operation.DestinationPath))
        {
            messages.Add($"Operation {operation.OperationId} requires a destination path.");
        }

        if (operation.Type is PatchOperationType.Rename &&
            (string.IsNullOrWhiteSpace(operation.SymbolName) || string.IsNullOrWhiteSpace(operation.NewName)))
        {
            messages.Add($"Operation {operation.OperationId} requires symbol and new names.");
        }

        if (operation.StartLine < 0 || operation.EndLine < 0)
        {
            messages.Add($"Operation {operation.OperationId} has an invalid line range.");
        }

        if (operation.Type is PatchOperationType.Extract &&
            (operation.StartLine <= 0 || operation.EndLine < operation.StartLine))
        {
            messages.Add($"Operation {operation.OperationId} requires a valid extract line range.");
        }
    }
}
