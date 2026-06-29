namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Executes workflow steps through confirmations, approvals, retry, and the framework pipeline.
/// </summary>
public sealed class WorkflowStepExecutor
{
    private readonly ITaskPipeline taskPipeline;
    private readonly RetryExecutor retryExecutor;
    private readonly IConfirmationService? confirmationService;
    private readonly IApprovalService? approvalService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowStepExecutor"/> class.
    /// </summary>
    /// <param name="taskPipeline">The framework task pipeline.</param>
    /// <param name="retryExecutor">The retry executor.</param>
    /// <param name="confirmationService">The optional confirmation service.</param>
    /// <param name="approvalService">The optional approval service.</param>
    public WorkflowStepExecutor(
        ITaskPipeline taskPipeline,
        RetryExecutor retryExecutor,
        IConfirmationService? confirmationService = null,
        IApprovalService? approvalService = null)
    {
        this.taskPipeline = taskPipeline;
        this.retryExecutor = retryExecutor;
        this.confirmationService = confirmationService;
        this.approvalService = approvalService;
    }

    /// <summary>
    /// Executes a workflow step.
    /// </summary>
    /// <param name="workflow">The workflow containing the step.</param>
    /// <param name="step">The step to execute.</param>
    /// <param name="cancellationToken">A token that cancels execution.</param>
    /// <returns>The step execution outcome.</returns>
    public async Task<StepExecutionOutcome> ExecuteAsync(
        Workflow workflow,
        WorkflowStep step,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);
        ArgumentNullException.ThrowIfNull(step);

        var entries = new List<string>();
        var gateResult = await RunGatesAsync(workflow, step, entries, cancellationToken);
        if (gateResult is not null)
        {
            return new StepExecutionOutcome { Step = step, Result = gateResult, HistoryEntries = entries };
        }

        var taskResult = await retryExecutor.ExecuteAsync(
            token => ExecuteTaskRequestAsync(step, token),
            step.RetryPolicy,
            (attempt, _, _) =>
            {
                entries.Add($"StepRetry:{step.Id}:{attempt}");
                return Task.CompletedTask;
            },
            cancellationToken);

        return new StepExecutionOutcome { Step = step, Result = taskResult, HistoryEntries = entries };
    }

    private async Task<TaskResult?> RunGatesAsync(
        Workflow workflow,
        WorkflowStep step,
        List<string> entries,
        CancellationToken cancellationToken)
    {
        if (step.ConfirmationRequest is not null)
        {
            var request = step.ConfirmationRequest;
            request.WorkflowId = workflow.Id;
            request.StepId = step.Id;
            entries.Add($"ConfirmationRequested:{step.Id}");
            var result = await RequireConfirmationAsync(request, cancellationToken);
            entries.Add(result.Confirmed ? $"ConfirmationAccepted:{step.Id}" : $"ConfirmationRejected:{step.Id}");
            if (!result.Confirmed)
            {
                return FailedStepResult(step, string.IsNullOrWhiteSpace(result.Reason) ? "Confirmation rejected." : result.Reason);
            }
        }

        if (step.ApprovalRequest is not null)
        {
            var request = step.ApprovalRequest;
            request.WorkflowId = workflow.Id;
            request.StepId = step.Id;
            entries.Add($"ApprovalRequested:{step.Id}");
            var result = await RequireApprovalAsync(request, cancellationToken);
            entries.Add(result.Approved ? $"ApprovalAccepted:{step.Id}" : $"ApprovalRejected:{step.Id}");
            if (!result.Approved)
            {
                return FailedStepResult(step, string.IsNullOrWhiteSpace(result.Reason) ? "Approval rejected." : result.Reason);
            }
        }

        return null;
    }

    private Task<ConfirmationResult> RequireConfirmationAsync(
        ConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        if (confirmationService is null)
        {
            return Task.FromResult(new ConfirmationResult
            {
                RequestId = request.RequestId,
                Confirmed = false,
                Reason = "Confirmation service is not configured."
            });
        }

        return confirmationService.RequestConfirmationAsync(request, cancellationToken);
    }

    private Task<ApprovalResult> RequireApprovalAsync(
        ApprovalRequest request,
        CancellationToken cancellationToken)
    {
        if (approvalService is null)
        {
            return Task.FromResult(new ApprovalResult
            {
                RequestId = request.RequestId,
                Approved = false,
                Reason = "Approval service is not configured."
            });
        }

        return approvalService.RequestApprovalAsync(request, cancellationToken);
    }

    private async Task<TaskResult> ExecuteTaskRequestAsync(WorkflowStep step, CancellationToken cancellationToken)
    {
        try
        {
            return await taskPipeline.ExecuteAsync(new TaskRequest
            {
                TaskType = step.TaskType,
                Input = step.Input,
                Parameters = step.Parameters
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TimeoutException ex)
        {
            return FailedStepResult(step, $"Timeout: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            return FailedStepResult(step, $"Network failure: {ex.Message}");
        }
        catch (IOException ex)
        {
            return FailedStepResult(step, $"Temporary tool failure: {ex.Message}");
        }
        catch (Exception ex)
        {
            return FailedStepResult(step, ex.Message);
        }
    }

    private static TaskResult FailedStepResult(WorkflowStep step, string errorMessage)
    {
        return new TaskResult
        {
            RequestId = step.Id,
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}
