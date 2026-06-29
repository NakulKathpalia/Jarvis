namespace Jarvis.Core.AI.Runtime;

/// <summary>
/// Identifies the category of an AI runtime failure.
/// </summary>
public enum AIErrorKind
{
    /// <summary>
    /// No failure occurred.
    /// </summary>
    None,

    /// <summary>
    /// The request was invalid.
    /// </summary>
    InvalidRequest,

    /// <summary>
    /// The provider could not be reached or is not running.
    /// </summary>
    ProviderUnavailable,

    /// <summary>
    /// The requested model was not available from the provider.
    /// </summary>
    ModelMissing,

    /// <summary>
    /// The request exceeded its timeout.
    /// </summary>
    Timeout,

    /// <summary>
    /// The provider returned an error.
    /// </summary>
    ProviderError,

    /// <summary>
    /// The runtime failed unexpectedly.
    /// </summary>
    RuntimeCrash
}
