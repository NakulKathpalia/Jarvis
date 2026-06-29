namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;

/// <summary>
/// Defines intent analysis for a user request.
/// </summary>
public interface IIntentAnalyzer
{
    /// <summary>
    /// Analyzes the supplied input and returns an intent result.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <returns>The intent result.</returns>
    IntentResult Analyze(string input);
}
