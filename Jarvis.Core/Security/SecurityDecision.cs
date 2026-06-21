namespace Jarvis.Security;

public sealed record SecurityDecision(
    bool Allowed,
    bool RequiresConfirmation,
    SecurityRiskLevel RiskLevel,
    string Reason,
    string SanitizedCommand,
    string SanitizedTarget)
{
    public static SecurityDecision Allow(SecurityRequest request, string command, string target, string reason) =>
        new(true, false, request.RiskLevel, reason, command, target);

    public static SecurityDecision RequireConfirmation(SecurityRequest request, string command, string target, string reason) =>
        new(false, true, request.RiskLevel, reason, command, target);

    public static SecurityDecision Block(SecurityRequest request, string command, string target, string reason) =>
        new(false, false, request.RiskLevel, reason, command, target);
}
