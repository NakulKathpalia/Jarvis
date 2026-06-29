namespace Jarvis.Core.Agents.Voice;

using Jarvis.Core.Framework.Agents;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Skills;

/// <summary>
/// Coordinates Voice Agent skills through the Jarvis framework pipeline.
/// </summary>
public sealed class VoiceAgent : AgentBase
{
    private readonly IReadOnlyCollection<ISkill> skills;

    /// <summary>
    /// Initializes a new instance of the <see cref="VoiceAgent"/> class.
    /// </summary>
    /// <param name="skills">The voice skills coordinated by this agent.</param>
    public VoiceAgent(IReadOnlyCollection<ISkill> skills)
        : base(new AgentDescriptor
        {
            Name = "VoiceAgent",
            DisplayName = "Voice Agent",
            Description = "Coordinates voice skills without directly accessing tools or hardware.",
            SupportedTaskTypes = ["Voice"]
        })
    {
        this.skills = skills;
    }

    /// <summary>
    /// Gets the configured voice skills.
    /// </summary>
    public IReadOnlyCollection<ISkill> Skills => skills;

    /// <inheritdoc />
    public override async Task<TaskResult> ExecuteAsync(
        AgentContext context,
        IToolExecutor toolExecutor,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ToolResult>();
        var selectedSkills = SelectSkills(context).ToArray();

        foreach (var skill in selectedSkills)
        {
            var result = await skill.ExecuteAsync(context, toolExecutor, cancellationToken);
            results.Add(result);

            if (!result.Succeeded)
            {
                return BuildResult(context, results, result.Output, result.ErrorMessage, succeeded: false);
            }
        }

        var output = results.LastOrDefault()?.Output ?? string.Empty;
        return BuildResult(context, results, output, string.Empty, succeeded: true);
    }

    private IEnumerable<ISkill> SelectSkills(AgentContext context)
    {
        if (!context.ExecutionContext.Request.Parameters.TryGetValue("VoiceSkills", out var value)
            || value is not IReadOnlyCollection<string> names
            || names.Count == 0)
        {
            return skills;
        }

        var requested = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        return skills.Where(skill => requested.Contains(skill.Name));
    }

    private TaskResult BuildResult(
        AgentContext context,
        List<ToolResult> toolResults,
        string output,
        string errorMessage,
        bool succeeded)
    {
        return new TaskResult
        {
            RequestId = context.ExecutionContext.Request.RequestId,
            AgentName = Name,
            Succeeded = succeeded,
            Output = output,
            ErrorMessage = errorMessage,
            ToolResults = toolResults
        };
    }
}
