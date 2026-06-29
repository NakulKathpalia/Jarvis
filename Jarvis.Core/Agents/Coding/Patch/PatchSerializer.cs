namespace Jarvis.Core.Agents.Coding.Patch;

using System.Text.Json;
using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Serializes deterministic patch plans.
/// </summary>
public sealed class PatchSerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>
    /// Serializes a patch plan.
    /// </summary>
    public string Serialize(PatchPlan plan)
    {
        return JsonSerializer.Serialize(plan, Options);
    }

    /// <summary>
    /// Deserializes a patch plan.
    /// </summary>
    public PatchPlan Deserialize(string serializedPlan)
    {
        return JsonSerializer.Deserialize<PatchPlan>(serializedPlan, Options) ?? new PatchPlan();
    }
}
