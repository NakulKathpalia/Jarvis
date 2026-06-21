namespace Jarvis.Security;

public sealed class SecurityService
{
    public const string ConfirmationPhrase = "yes run it";

    private readonly InputValidator _inputValidator;
    private readonly PermissionService _permissionService;
    private readonly AuditLogger _auditLogger;

    public SecurityService(
        InputValidator inputValidator,
        PermissionService permissionService,
        AuditLogger auditLogger)
    {
        _inputValidator = inputValidator;
        _permissionService = permissionService;
        _auditLogger = auditLogger;
    }

    public async Task<SecurityDecision> ValidateAsync(
        SecurityRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = _inputValidator.Validate(request);
        if (!validation.IsValid)
        {
            await _auditLogger.LogAsync(request, SecurityAuditResult.Blocked, validation.Reason, cancellationToken);
            return SecurityDecision.Block(request, validation.SanitizedCommand, validation.SanitizedTarget, validation.Reason);
        }

        if (!_permissionService.CanExecute(request, out var permissionReason))
        {
            await _auditLogger.LogAsync(request, SecurityAuditResult.Blocked, permissionReason, cancellationToken);
            return SecurityDecision.Block(request, validation.SanitizedCommand, validation.SanitizedTarget, permissionReason);
        }

        if (request.RiskLevel == SecurityRiskLevel.Dangerous)
        {
            const string reason = "Dangerous action requires exact confirmation phrase.";
            await _auditLogger.LogAsync(request, SecurityAuditResult.ConfirmationRequired, reason, cancellationToken);
            return SecurityDecision.RequireConfirmation(request, validation.SanitizedCommand, validation.SanitizedTarget, reason);
        }

        await _auditLogger.LogAsync(request, SecurityAuditResult.Allowed, "Security pipeline allowed execution.", cancellationToken);
        return SecurityDecision.Allow(request, validation.SanitizedCommand, validation.SanitizedTarget, "Security pipeline allowed execution.");
    }

    public Task AuditExecutionAsync(
        SecurityRequest request,
        bool succeeded,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return _auditLogger.LogAsync(
            request,
            succeeded ? SecurityAuditResult.Allowed : SecurityAuditResult.Failed,
            reason,
            cancellationToken);
    }
}
