namespace Jarvis.Memory;

public sealed class MemoryScoringService
{
    public int NormalizeImportance(int importance) => Math.Clamp(importance, 1, 10);

    public int NormalizeConfidence(int confidence) => Math.Clamp(confidence, 1, 10);
}
