namespace Jarvis.Security;

public sealed record PermissionDefinition(
    string Key,
    string Description,
    string Category,
    SecurityRiskLevel RiskLevel = SecurityRiskLevel.Safe);
