namespace Jarvis.Core.Agents.Coding.Experience;

/// <summary>
/// Defines categories for coding experience entries.
/// </summary>
public enum ExperienceCategory
{
    /// <summary>
    /// General coding experience.
    /// </summary>
    General,

    /// <summary>
    /// Successful implementation experience.
    /// </summary>
    SuccessfulPatch,

    /// <summary>
    /// Failed implementation experience.
    /// </summary>
    FailedPatch,

    /// <summary>
    /// Successful build experience.
    /// </summary>
    SuccessfulBuild,

    /// <summary>
    /// Compiler error experience.
    /// </summary>
    CompilerError,

    /// <summary>
    /// Review or quality experience.
    /// </summary>
    Review
}
