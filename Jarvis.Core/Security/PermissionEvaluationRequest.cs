using Jarvis.Users;

namespace Jarvis.Security;

public sealed record PermissionEvaluationRequest(
    string UserId,
    IReadOnlyCollection<UserRole> Roles,
    string Permission,
    string Resource = "",
    string Action = "",
    SecurityRiskLevel RiskLevel = SecurityRiskLevel.Safe);
