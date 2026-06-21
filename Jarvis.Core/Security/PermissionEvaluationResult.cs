namespace Jarvis.Security;

public sealed record PermissionEvaluationResult(
    PermissionDecision Decision,
    string Permission,
    string Reason)
{
    public bool IsAllowed => Decision == PermissionDecision.Allow;
}
