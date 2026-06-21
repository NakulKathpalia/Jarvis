namespace Jarvis.Security;

public sealed record InputValidationResult(
    bool IsValid,
    string SanitizedCommand,
    string SanitizedTarget,
    string Reason)
{
    public static InputValidationResult Valid(string command, string target) =>
        new(true, command, target, "Input passed validation.");

    public static InputValidationResult Invalid(string command, string target, string reason) =>
        new(false, command, target, reason);
}
