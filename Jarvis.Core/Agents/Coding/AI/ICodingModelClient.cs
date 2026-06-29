namespace Jarvis.Core.Agents.Coding.AI;

/// <summary>
/// Defines a local coding model client.
/// </summary>
public interface ICodingModelClient
{
    /// <summary>
    /// Sends a coding prompt to a local model.
    /// </summary>
    /// <param name="request">The model request.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The model response.</returns>
    Task<CodingModelResponse> GenerateAsync(
        CodingModelRequest request,
        CancellationToken cancellationToken = default);
}
