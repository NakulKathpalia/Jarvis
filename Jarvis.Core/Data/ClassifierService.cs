namespace Jarvis.Data;

public sealed class ClassifierService
{
    public bool LooksLikeCommand(string input)
    {
        return input.TrimStart().StartsWith('/');
    }
}
