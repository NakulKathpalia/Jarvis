namespace Jarvis.Core.Agents.Coding.Patch;

using Jarvis.Core.Agents.Coding.Models;

/// <summary>
/// Coordinates deterministic patch validation, preview, and execution.
/// </summary>
public sealed class PatchEngine
{
    private readonly PatchValidator validator;
    private readonly PatchPreviewBuilder previewBuilder;
    private readonly PatchExecutor executor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PatchEngine"/> class.
    /// </summary>
    public PatchEngine()
        : this(new PatchValidator(), new PatchPreviewBuilder(), new PatchExecutor())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PatchEngine"/> class.
    /// </summary>
    public PatchEngine(PatchValidator validator, PatchPreviewBuilder previewBuilder, PatchExecutor executor)
    {
        this.validator = validator;
        this.previewBuilder = previewBuilder;
        this.executor = executor;
    }

    /// <summary>
    /// Creates a validated patch plan.
    /// </summary>
    public PatchPlan CreatePlan(PatchRequest request)
    {
        var messages = validator.Validate(request);
        if (messages.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, messages));
        }

        return new PatchPlan
        {
            DryRun = request.DryRun,
            Operations = request.Operations
        };
    }

    /// <summary>
    /// Builds a preview for a patch plan.
    /// </summary>
    public PatchPreview Preview(PatchPlan plan)
    {
        return previewBuilder.Build(plan);
    }

    /// <summary>
    /// Executes a patch request.
    /// </summary>
    public PatchResult Execute(PatchRequest request)
    {
        var plan = CreatePlan(request);
        var preview = Preview(plan);
        return executor.Execute(plan, preview);
    }
}
