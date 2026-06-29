namespace Jarvis.Core.Brain.Interfaces;

using Jarvis.Core.Brain.Models;

/// <summary>
/// Defines task analysis before workflow planning.
/// </summary>
public interface ITaskAnalyzer
{
    /// <summary>
    /// Analyzes the input and intent.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <param name="intent">The detected intent.</param>
    /// <returns>The task analysis.</returns>
    TaskAnalysis Analyze(string input, IntentResult intent);
}
