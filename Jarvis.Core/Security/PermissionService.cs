using Jarvis.Users;

namespace Jarvis.Security;

public sealed class PermissionService
{
    private readonly JarvisUserContext _userContext;
    private readonly PermissionResolver _resolver;

    public PermissionService()
        : this(new JarvisUserContext(), new PermissionResolver())
    {
    }

    public PermissionService(JarvisUserContext userContext, PermissionResolver? resolver = null)
    {
        _userContext = userContext;
        _resolver = resolver ?? new PermissionResolver();
    }

    public bool CanExecute(SecurityRequest request, out string reason)
    {
        if (request.RiskLevel == SecurityRiskLevel.Blocked)
        {
            reason = "Blocked risk level cannot be executed.";
            return false;
        }

        var permission = request.RiskLevel == SecurityRiskLevel.Dangerous
            ? PermissionDefinitions.CommandsExecuteDestructive
            : PermissionDefinitions.CommandsExecute;
        var result = Evaluate(new PermissionEvaluationRequest(
            _userContext.UserId,
            _userContext.Roles,
            permission,
            request.Target,
            request.Command,
            request.RiskLevel));

        reason = result.Reason;
        return result.Decision != PermissionDecision.Deny;
    }

    public PermissionEvaluationResult Evaluate(
        string permission,
        SecurityRiskLevel riskLevel = SecurityRiskLevel.Safe,
        string resource = "",
        string action = "")
    {
        return Evaluate(new PermissionEvaluationRequest(
            _userContext.UserId,
            _userContext.Roles,
            permission,
            resource,
            action,
            riskLevel));
    }

    public PermissionEvaluationResult Evaluate(PermissionEvaluationRequest request)
    {
        if (!PermissionDefinitions.Exists(request.Permission))
        {
            return new PermissionEvaluationResult(
                PermissionDecision.Deny,
                request.Permission,
                $"Unknown permission '{request.Permission}'.");
        }

        var resolved = _resolver.Resolve(request.Roles);
        if (resolved.Denied.Contains(request.Permission))
        {
            return new PermissionEvaluationResult(
                PermissionDecision.Deny,
                request.Permission,
                $"Permission '{request.Permission}' is explicitly denied.");
        }

        if (!resolved.Allowed.Contains(request.Permission))
        {
            return new PermissionEvaluationResult(
                PermissionDecision.Deny,
                request.Permission,
                $"Permission '{request.Permission}' is not granted.");
        }

        if (request.RiskLevel == SecurityRiskLevel.Dangerous)
        {
            return new PermissionEvaluationResult(
                PermissionDecision.RequireConfirmation,
                request.Permission,
                $"Permission '{request.Permission}' is granted but requires confirmation.");
        }

        return new PermissionEvaluationResult(
            PermissionDecision.Allow,
            request.Permission,
            $"Permission '{request.Permission}' granted.");
    }
}
