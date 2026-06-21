namespace Jarvis.Security;

public sealed record SecurityRequest(
    string Source,
    string Command,
    string Intent,
    string Target,
    SecurityRiskLevel RiskLevel);
