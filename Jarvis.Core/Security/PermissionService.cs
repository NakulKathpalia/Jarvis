namespace Jarvis.Security;

public sealed class PermissionService
{
    public bool CanExecute(SecurityRequest request, out string reason)
    {
        if (request.RiskLevel == SecurityRiskLevel.Blocked)
        {
            reason = "Blocked risk level cannot be executed.";
            return false;
        }

        reason = "Permission granted by local default policy.";
        return true;
    }
}
