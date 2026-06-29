namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Defines serialization contracts for workflow snapshots.
/// </summary>
public interface WorkflowSerializer
{
    /// <summary>
    /// Serializes a workflow snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>The serialized snapshot.</returns>
    string Serialize(WorkflowSnapshot snapshot);

    /// <summary>
    /// Deserializes a workflow snapshot.
    /// </summary>
    /// <param name="serializedSnapshot">The serialized snapshot.</param>
    /// <returns>The deserialized workflow snapshot.</returns>
    WorkflowSnapshot Deserialize(string serializedSnapshot);
}
