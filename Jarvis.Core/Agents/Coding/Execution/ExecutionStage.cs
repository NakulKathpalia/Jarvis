namespace Jarvis.Core.Agents.Coding.Execution;

/// <summary>
/// Defines coding execution stages.
/// </summary>
public enum ExecutionStage
{
    RequestReceived,
    RepositoryScan,
    ContextBuilding,
    Planning,
    AIRequest,
    Review,
    PatchPreview,
    Approval,
    PatchApply,
    Build,
    Tests,
    GitStatus,
    Finished
}
