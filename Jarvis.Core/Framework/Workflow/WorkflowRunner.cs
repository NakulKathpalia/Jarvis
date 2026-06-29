namespace Jarvis.Core.Framework.Workflow;

using Jarvis.Core.Core.Logging;
using Jarvis.Core.Framework.Events;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Routing;

/// <summary>
/// Executes workflow steps through the framework task pipeline.
/// </summary>
public sealed class WorkflowRunner : IWorkflowRunner
{
    private readonly ITaskPipeline taskPipeline;
    private readonly IEventBus? eventBus;
    private readonly WorkflowStateValidator stateValidator;
    private readonly IFrameworkLogger? logger;
    private readonly ConditionEvaluator conditionEvaluator;
    private readonly RetryExecutor retryExecutor;
    private readonly WorkflowValidator workflowValidator;
    private readonly WorkflowStepScheduler stepScheduler;
    private readonly IConfirmationService? confirmationService;
    private readonly IApprovalService? approvalService;
    private readonly WorkflowControl control;
    private readonly WorkflowMetricsCollector metricsCollector;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowRunner"/> class.
    /// </summary>
    /// <param name="taskPipeline">The framework task pipeline.</param>
    /// <param name="eventBus">The optional framework event bus.</param>
    /// <param name="stateValidator">The optional workflow state validator.</param>
    /// <param name="logger">The optional framework logger.</param>
    /// <param name="conditionEvaluator">The optional condition evaluator.</param>
    /// <param name="retryExecutor">The optional retry executor.</param>
    /// <param name="workflowValidator">The optional workflow validator.</param>
    /// <param name="stepScheduler">The optional step scheduler.</param>
    /// <param name="confirmationService">The optional confirmation service.</param>
    /// <param name="approvalService">The optional approval service.</param>
    /// <param name="control">The optional workflow control state.</param>
    /// <param name="metricsCollector">The optional metrics collector.</param>
    public WorkflowRunner(
        ITaskPipeline taskPipeline,
        IEventBus? eventBus = null,
        WorkflowStateValidator? stateValidator = null,
        IFrameworkLogger? logger = null,
        ConditionEvaluator? conditionEvaluator = null,
        RetryExecutor? retryExecutor = null,
        WorkflowValidator? workflowValidator = null,
        WorkflowStepScheduler? stepScheduler = null,
        IConfirmationService? confirmationService = null,
        IApprovalService? approvalService = null,
        WorkflowControl? control = null,
        WorkflowMetricsCollector? metricsCollector = null)
    {
        this.taskPipeline = taskPipeline;
        this.eventBus = eventBus;
        this.stateValidator = stateValidator ?? new WorkflowStateValidator();
        this.logger = logger;
        this.conditionEvaluator = conditionEvaluator ?? new ConditionEvaluator();
        this.retryExecutor = retryExecutor ?? new RetryExecutor();
        this.workflowValidator = workflowValidator ?? new WorkflowValidator();
        this.stepScheduler = stepScheduler ?? new WorkflowStepScheduler();
        this.confirmationService = confirmationService;
        this.approvalService = approvalService;
        this.control = control ?? new WorkflowControl();
        this.metricsCollector = metricsCollector ?? new WorkflowMetricsCollector();
    }

    /// <inheritdoc />
    public async Task<WorkflowResult> RunAsync(Workflow workflow, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        var history = new WorkflowHistory();
        var result = new WorkflowResult
        {
            WorkflowId = workflow.Id,
            History = history
        };

        var stateMachine = new WorkflowStateMachine(workflow.State, stateValidator);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            workflowValidator.Validate(workflow);
            control.ResetProgress(workflow.Steps.Count);
            history.StartedAtUtc = DateTimeOffset.UtcNow;
            result.StartedAtUtc = history.StartedAtUtc;

            await PublishAsync(workflow, "WorkflowCreated", workflow, cancellationToken);
            Transition(workflow, stateMachine, WorkflowState.Queued, "Workflow queued", history);
            Transition(workflow, stateMachine, WorkflowState.Ready, "Workflow validated", history);
            Transition(workflow, stateMachine, WorkflowState.Running, "Workflow started", history);
            await PublishAsync(workflow, "WorkflowStarted", workflow, cancellationToken);

            var executionState = new WorkflowExecutionState();
            control.Attach(linkedCancellation, executionState, workflow.Id, workflow.State);
            await ExecuteWorkflowAsync(workflow, result, history, executionState, stateMachine, linkedCancellation.Token);

            Transition(workflow, stateMachine, WorkflowState.Completed, "Workflow completed", history);
            result.State = workflow.State;
            await PublishAsync(workflow, "WorkflowCompleted", result, cancellationToken);
            result.Succeeded = true;
        }
        catch (OperationCanceledException)
        {
            Transition(workflow, stateMachine, WorkflowState.Cancelled, "Workflow cancelled", history);
            result.State = workflow.State;
            result.Errors.Add("Workflow cancelled.");
            await PublishAsync(workflow, "WorkflowCancelled", result, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Transition(workflow, stateMachine, WorkflowState.Failed, "Workflow failed", history);
            result.State = workflow.State;
            result.Errors.Add(ex.Message);
            history.Errors.Add(ex.Message);
            await PublishAsync(workflow, "WorkflowFailed", result, CancellationToken.None);
        }
        finally
        {
            control.Detach();
            history.FinishedAtUtc = DateTimeOffset.UtcNow;
            history.Status = workflow.State;
            result.FinishedAtUtc = history.FinishedAtUtc;
            result.State = workflow.State;
            metricsCollector.RecordWorkflow(result, workflow.Steps.Count);
            await LogAsync(workflow, result, CancellationToken.None);
        }

        return result;
    }

    /// <inheritdoc />
    public void Pause()
    {
        control.Pause();
    }

    /// <inheritdoc />
    public void Resume()
    {
        control.Resume();
    }

    /// <inheritdoc />
    public void Cancel()
    {
        control.Cancel();
    }

    /// <inheritdoc />
    public bool CancelStep(string stepId)
    {
        return control.CancelStep(stepId);
    }

    /// <inheritdoc />
    public WorkflowProgress GetProgress()
    {
        return control.GetProgress();
    }

    /// <inheritdoc />
    public WorkflowSnapshot CreateCheckpoint()
    {
        return control.CreateCheckpoint();
    }

    /// <inheritdoc />
    public WorkflowMetrics GetMetrics()
    {
        return metricsCollector.GetSnapshot();
    }

    private async Task ExecuteWorkflowAsync(
        Workflow workflow,
        WorkflowResult result,
        WorkflowHistory history,
        WorkflowExecutionState executionState,
        WorkflowStateMachine stateMachine,
        CancellationToken cancellationToken)
    {
        var totalSteps = workflow.Steps.Count;
        while (executionState.CompletedStepIds.Count + executionState.SkippedStepIds.Count < totalSteps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WaitIfPausedAsync(workflow, stateMachine, history, cancellationToken);
            ApplyBranches(workflow, history, executionState);

            var readySteps = stepScheduler.GetReadySteps(workflow.Steps, executionState);
            if (readySteps.Count == 0)
            {
                throw new InvalidOperationException("Workflow dependencies could not be resolved.");
            }

            metricsCollector.RecordParallelBatch(readySteps.Count);
            var batchHasGate = readySteps.Any(step => step.ConfirmationRequest is not null || step.ApprovalRequest is not null);
            if (batchHasGate)
            {
                Transition(workflow, stateMachine, WorkflowState.Waiting, "Workflow waiting for confirmation or approval", history);
            }

            foreach (var step in readySteps)
            {
                executionState.RunningStepIds.Add(step.Id);
                await PublishAsync(workflow, "StepStarted", step, cancellationToken);
                AddHistory(history, $"StepStarted:{step.Id}");
            }

            control.UpdateProgress(workflow.Steps.Count, executionState);
            var stepTasks = readySteps
                .Select(step => ExecuteStepAsync(workflow, step, executionState, cancellationToken))
                .ToList();

            var outcomes = await Task.WhenAll(stepTasks);

            if (batchHasGate)
            {
                Transition(workflow, stateMachine, WorkflowState.Running, "Workflow resumed after confirmation or approval", history);
            }

            foreach (var outcome in outcomes.OrderBy(item => item.Step.Order).ThenBy(item => item.Step.Id, StringComparer.OrdinalIgnoreCase))
            {
                RecordStepOutcome(workflow, outcome, result, history, executionState);
                foreach (var entry in outcome.HistoryEntries)
                {
                    AddHistory(history, entry);
                }

                metricsCollector.RecordRetries(outcome.HistoryEntries.Count(entry => entry.StartsWith("StepRetry:", StringComparison.OrdinalIgnoreCase)));
                await PublishAsync(workflow, "StepCompleted", outcome.Result, cancellationToken);
                control.UpdateProgress(workflow.Steps.Count, executionState);

                if (workflow.Options.StopOnFailure && !outcome.Result.Succeeded)
                {
                    throw new InvalidOperationException(outcome.Result.ErrorMessage);
                }
            }
        }
    }

    private void ApplyBranches(
        Workflow workflow,
        WorkflowHistory history,
        WorkflowExecutionState executionState)
    {
        foreach (var branch in workflow.Branches.Where(branch => !executionState.EvaluatedBranchIds.Contains(branch.Id)))
        {
            if (!executionState.StepResults.ContainsKey(branch.Condition.SourceStepId))
            {
                continue;
            }

            var conditionMatched = conditionEvaluator.Evaluate(branch.Condition, executionState.StepResults);
            var skippedSteps = conditionMatched ? branch.ElseStepIds : branch.IfStepIds;
            foreach (var skippedStep in skippedSteps)
            {
                executionState.SkippedStepIds.Add(skippedStep);
                AddHistory(history, $"StepSkipped:{skippedStep}");
            }

            executionState.EvaluatedBranchIds.Add(branch.Id);
            AddHistory(history, conditionMatched ? $"BranchIf:{branch.Id}" : $"BranchElse:{branch.Id}");
        }
    }

    private async Task<StepExecutionOutcome> ExecuteStepAsync(
        Workflow workflow,
        WorkflowStep step,
        WorkflowExecutionState executionState,
        CancellationToken cancellationToken)
    {
        using var stepCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        executionState.StepCancellationSources[step.Id] = stepCancellation;
        try
        {
            var executor = new WorkflowStepExecutor(
                taskPipeline,
                retryExecutor,
                confirmationService,
                approvalService);
            return await executor.ExecuteAsync(workflow, step, stepCancellation.Token);
        }
        finally
        {
            executionState.StepCancellationSources.Remove(step.Id);
        }
    }

    private static void RecordStepOutcome(
        Workflow workflow,
        StepExecutionOutcome outcome,
        WorkflowResult result,
        WorkflowHistory history,
        WorkflowExecutionState executionState)
    {
        var taskResult = outcome.Result;
        result.StepResults.Add(taskResult);
        result.Outputs[outcome.Step.Id] = taskResult.Output;
        history.Outputs[outcome.Step.Id] = taskResult.Output;
        executionState.StepResults[outcome.Step.Id] = taskResult;
        executionState.RunningStepIds.Remove(outcome.Step.Id);

        AddHistory(history, taskResult.Succeeded ? $"StepCompleted:{outcome.Step.Id}" : $"StepFailed:{outcome.Step.Id}");

        if (!taskResult.Succeeded && !string.IsNullOrWhiteSpace(taskResult.ErrorMessage))
        {
            executionState.FailedStepId = outcome.Step.Id;
            result.Errors.Add(taskResult.ErrorMessage);
            history.Errors.Add(taskResult.ErrorMessage);
        }

        if (taskResult.Succeeded || !workflow.Options.StopOnFailure)
        {
            executionState.CompletedStepIds.Add(outcome.Step.Id);
        }
    }

    private async Task WaitIfPausedAsync(
        Workflow workflow,
        WorkflowStateMachine stateMachine,
        WorkflowHistory history,
        CancellationToken cancellationToken)
    {
        var waitTask = control.GetPauseTask();
        if (waitTask is null)
        {
            return;
        }

        Transition(workflow, stateMachine, WorkflowState.Paused, "Workflow paused", history);
        await waitTask.WaitAsync(cancellationToken);
        Transition(workflow, stateMachine, WorkflowState.Running, "Workflow resumed", history);
    }

    private void Transition(
        Workflow workflow,
        WorkflowStateMachine stateMachine,
        WorkflowState state,
        string reason,
        WorkflowHistory history)
    {
        stateMachine.TransitionTo(state, reason);
        workflow.State = stateMachine.CurrentState;
        control.UpdateState(workflow.State);
        history.Entries.Add($"StateChanged:{state}");
    }

    private static void AddHistory(WorkflowHistory history, string entry)
    {
        lock (history)
        {
            history.Entries.Add(entry);
        }
    }

    private Task PublishAsync(Workflow workflow, string name, object payload, CancellationToken cancellationToken)
    {
        if (eventBus is null || !workflow.Options.PublishEvents)
        {
            return Task.CompletedTask;
        }

        return eventBus.PublishAsync(new FrameworkEvent
        {
            Name = name,
            CorrelationId = workflow.Id,
            Payload = payload
        }, cancellationToken);
    }

    private Task LogAsync(Workflow workflow, WorkflowResult result, CancellationToken cancellationToken)
    {
        if (logger is null)
        {
            return Task.CompletedTask;
        }

        return logger.LogAsync(new FrameworkLogEntry
        {
            CorrelationId = workflow.Id,
            Message = $"Workflow '{workflow.Id}' finished with state '{result.State}'."
        }, cancellationToken);
    }

}
