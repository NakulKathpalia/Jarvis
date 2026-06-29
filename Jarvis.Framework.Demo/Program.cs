using Jarvis.Core.Agents.Echo;
using Jarvis.Core.Agents.Coding;
using Jarvis.Core.Brain.Models;
using Jarvis.Core.Framework.Agents;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Events;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Planner;
using Jarvis.Core.Framework.Registry;
using Jarvis.Core.Framework.Routing;
using Jarvis.Core.Framework.Workflow;

var registry = new AgentRegistry();
registry.Register(new EchoAgent());
registry.Register(new CodingAgent());

var planner = new TaskPlanner();
var contextManager = new ContextManager();
var toolExecutor = new ToolExecutor();
var agentManager = new AgentManager(planner, registry, contextManager, toolExecutor);
var eventBus = new EventBus();
var observedEvents = new List<string>();
var observedEventDetails = new List<string>();

foreach (var eventName in new[]
{
    "WorkflowCreated",
    "WorkflowStarted",
    "StepStarted",
    "StepCompleted",
    "WorkflowCompleted",
    "WorkflowFailed",
    "WorkflowCancelled"
})
{
    eventBus.Subscribe(eventName, (frameworkEvent, _) =>
    {
        observedEvents.Add(frameworkEvent.Name);
        observedEventDetails.Add($"{frameworkEvent.Name}:{DescribePayload(frameworkEvent.Payload)}");
        return Task.CompletedTask;
    });
}

var pipeline = new TaskPipeline(agentManager, eventBus);
var transientPipeline = new TransientFailurePipeline(pipeline);

var result = await pipeline.ExecuteAsync(new TaskRequest
{
    TaskType = "Echo",
    Input = "hello jarvis"
});

if (!result.Succeeded || result.Output != "hello jarvis")
{
    Console.Error.WriteLine("EchoAgent pipeline validation failed.");
    Console.Error.WriteLine($"Succeeded: {result.Succeeded}");
    Console.Error.WriteLine($"Output: {result.Output}");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("EchoAgent pipeline validation passed.");
Console.WriteLine(result.Output);

var executionPlan = new ExecutionPlan
{
    Input = "hello jarvis",
    Steps =
    [
        new ExecutionStep
        {
            StepId = "step1",
            Order = 1,
            StepType = "Echo",
            Input = "hello jarvis"
        },
        new ExecutionStep
        {
            StepId = "step2",
            Order = 2,
            StepType = "Echo",
            Input = "hello jarvis"
        },
        new ExecutionStep
        {
            StepId = "step3",
            Order = 3,
            StepType = "Echo",
            Input = "hello jarvis"
        },
        new ExecutionStep
        {
            StepId = "stepElse",
            Order = 4,
            StepType = "Echo",
            Input = "else branch"
        },
        new ExecutionStep
        {
            StepId = "step4",
            Order = 5,
            StepType = "Echo",
            Input = "hello jarvis"
        }
    ]
};

var workflow = Workflow.FromExecutionPlan(executionPlan);
workflow.Id = "echo-workflow-p2";
workflow.Steps[1].Dependencies.Add("step1");
workflow.Steps[2].Dependencies.Add("step2");
workflow.Steps[2].Dependencies.Clear();
workflow.Steps[2].Dependencies.Add("step1");
workflow.Steps[3].Dependencies.Add("step1");
workflow.Steps[4].Dependencies.Add("step2");
workflow.Steps[4].Dependencies.Add("step3");
workflow.Steps[4].Dependencies.Add("stepElse");
workflow.Steps[1].Parameters["FailOnceKey"] = "step2-transient";
workflow.Steps[1].RetryPolicy = new RetryPolicy
{
    RetryCount = 1,
    RetryDelay = TimeSpan.Zero
};
workflow.Branches.Add(new WorkflowBranch
{
    Id = "branch1",
    Condition = new WorkflowCondition
    {
        SourceStepId = "step1",
        ExpectedSucceeded = true,
        OutputEquals = "hello jarvis"
    },
    IfStepIds = ["step2", "step3"],
    ElseStepIds = ["stepElse"],
    EndStepId = "step4"
});

observedEvents.Clear();
observedEventDetails.Clear();

var workflowRunner = new WorkflowRunner(transientPipeline, eventBus);
var workflowResult = await workflowRunner.RunAsync(workflow);

if (!workflowResult.Succeeded ||
    workflowResult.State != WorkflowState.Completed ||
    workflowResult.StepResults.Count != 4 ||
    workflowResult.StepResults.Any(step => step.Output != "hello jarvis") ||
    !workflowResult.History.Entries.Contains("BranchIf:branch1") ||
    !workflowResult.History.Entries.Contains("StepSkipped:stepElse") ||
    !workflowResult.History.Entries.Contains("StepRetry:step2:1") ||
    !workflowResult.History.Entries.Contains("StepCompleted:step4") ||
    !observedEvents.Contains("WorkflowCompleted"))
{
    Console.Error.WriteLine("Workflow P2 validation failed.");
    Console.Error.WriteLine($"Succeeded: {workflowResult.Succeeded}");
    Console.Error.WriteLine($"State: {workflowResult.State}");
    Console.Error.WriteLine($"Step results: {workflowResult.StepResults.Count}");
    Console.Error.WriteLine($"Events: {string.Join(", ", observedEventDetails)}");
    Console.Error.WriteLine($"History: {string.Join(", ", workflowResult.History.Entries)}");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Workflow P2 validation passed.");
Console.WriteLine($"Workflow state: {workflowResult.State}");
Console.WriteLine($"History entries: {workflowResult.History.Entries.Count}");
Console.WriteLine($"Events observed: {string.Join(", ", observedEventDetails)}");

var p3ConfirmationService = new ConfirmationService();
var p3ApprovalService = new ApprovalService();
var p3Pipeline = new ValidationPipeline();
var p3Runner = new WorkflowRunner(
    p3Pipeline,
    eventBus,
    confirmationService: p3ConfirmationService,
    approvalService: p3ApprovalService);

var confirmationRequest = new ConfirmationRequest
{
    RequestId = "confirm-tests",
    Message = "Continue to tests?"
};
var approvalRequest = new ApprovalRequest
{
    RequestId = "approve-deploy",
    Action = "Deploy",
    Message = "Approve deploy?"
};
var p3Workflow = new Workflow
{
    Id = "workflow-p3-validation",
    Steps =
    [
        new WorkflowStep
        {
            Id = "build",
            Order = 1,
            TaskType = "Build",
            Input = "Build",
            Parameters = { ["DelayMs"] = 100 }
        },
        new WorkflowStep
        {
            Id = "confirmation",
            Order = 2,
            TaskType = "Confirm",
            Input = "Confirmation",
            Dependencies = ["build"],
            ConfirmationRequest = confirmationRequest
        },
        new WorkflowStep
        {
            Id = "tests",
            Order = 3,
            TaskType = "Tests",
            Input = "Tests",
            Dependencies = ["confirmation"]
        },
        new WorkflowStep
        {
            Id = "approval",
            Order = 4,
            TaskType = "Approval",
            Input = "Approval",
            Dependencies = ["tests"],
            ApprovalRequest = approvalRequest
        },
        new WorkflowStep
        {
            Id = "deploy",
            Order = 5,
            TaskType = "Deploy",
            Input = "Deploy",
            Dependencies = ["approval"]
        }
    ]
};

var p3RunTask = p3Runner.RunAsync(p3Workflow);
await WaitUntilAsync(() => p3Runner.GetProgress().RunningStep == "build");
p3Runner.Pause();
await WaitUntilAsync(() => p3Runner.GetProgress().CompletedSteps >= 1);
var pausedProgress = p3Runner.GetProgress();
if (pausedProgress.CompletedSteps != 1 || pausedProgress.EstimatedRemainingSteps != 4)
{
    Console.Error.WriteLine("Workflow P3 pause/progress validation failed.");
    Environment.ExitCode = 1;
    return;
}

p3Runner.Resume();
await WaitUntilAsync(() => p3ConfirmationService.Complete(new ConfirmationResult
{
    RequestId = confirmationRequest.RequestId,
    Confirmed = true
}));
await WaitUntilAsync(() => p3ApprovalService.Complete(new ApprovalResult
{
    RequestId = approvalRequest.RequestId,
    Approved = true
}));
var p3Result = await p3RunTask;

if (!p3Result.Succeeded ||
    p3Result.State != WorkflowState.Completed ||
    p3Result.StepResults.Count != 5 ||
    !p3Result.History.Entries.Contains("ConfirmationAccepted:confirmation") ||
    !p3Result.History.Entries.Contains("ApprovalAccepted:approval") ||
    p3Runner.GetProgress().ProgressPercent != 100)
{
    Console.Error.WriteLine("Workflow P3 confirmation/approval validation failed.");
    Console.Error.WriteLine($"Succeeded: {p3Result.Succeeded}");
    Console.Error.WriteLine($"State: {p3Result.State}");
    Console.Error.WriteLine($"Progress: {p3Runner.GetProgress().ProgressPercent}");
    Console.Error.WriteLine($"History: {string.Join(", ", p3Result.History.Entries)}");
    Environment.ExitCode = 1;
    return;
}

var cancelRunner = new WorkflowRunner(p3Pipeline, eventBus);
var cancelWorkflow = CreateSingleStepWorkflow("workflow-cancel-validation", "cancel-build", 5000);
var cancelTask = cancelRunner.RunAsync(cancelWorkflow);
await WaitUntilAsync(() => cancelRunner.GetProgress().RunningStep == "cancel-build");
cancelRunner.Cancel();
var cancelResult = await cancelTask;
if (cancelResult.State != WorkflowState.Cancelled)
{
    Console.Error.WriteLine("Workflow P3 cancel validation failed.");
    Environment.ExitCode = 1;
    return;
}

var cancelStepRunner = new WorkflowRunner(p3Pipeline, eventBus);
var cancelStepWorkflow = CreateSingleStepWorkflow("workflow-cancel-step-validation", "cancel-step-build", 5000);
var cancelStepTask = cancelStepRunner.RunAsync(cancelStepWorkflow);
await WaitUntilAsync(() => cancelStepRunner.GetProgress().RunningStep == "cancel-step-build");
if (!cancelStepRunner.CancelStep("cancel-step-build"))
{
    Console.Error.WriteLine("Workflow P3 cancel step request failed.");
    Environment.ExitCode = 1;
    return;
}

var cancelStepResult = await cancelStepTask;
if (cancelStepResult.State != WorkflowState.Cancelled)
{
    Console.Error.WriteLine("Workflow P3 cancel step validation failed.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Workflow P3 validation passed.");
Console.WriteLine($"P3 progress: {p3Runner.GetProgress().ProgressPercent}%");
Console.WriteLine($"P3 cancel state: {cancelResult.State}");
Console.WriteLine($"P3 cancel step state: {cancelStepResult.State}");

var templateRegistry = new WorkflowTemplateRegistry();
templateRegistry.Register(new WorkflowTemplate
{
    TemplateId = "build-test-deploy",
    Name = "Build Test Deploy",
    Description = "Reusable validation workflow template.",
    Workflow = p3Workflow
});

var inspector = new WorkflowInspector();
var diagnostics = inspector.Inspect(p3Workflow, p3Runner.GetProgress(), p3Runner.GetMetrics());
if (!templateRegistry.TryGet("build-test-deploy", out var registeredTemplate) ||
    registeredTemplate is null ||
    !diagnostics.IsValid)
{
    Console.Error.WriteLine("Workflow P4 template/diagnostics validation failed.");
    Environment.ExitCode = 1;
    return;
}

var checkpointRunner = new WorkflowRunner(p3Pipeline, eventBus);
var checkpointWorkflow = CreateSingleStepWorkflow("workflow-checkpoint-validation", "checkpoint-build", 250);
var checkpointTask = checkpointRunner.RunAsync(checkpointWorkflow);
await WaitUntilAsync(() => checkpointRunner.GetProgress().RunningStep == "checkpoint-build");
var checkpoint = checkpointRunner.CreateCheckpoint();
var checkpointResult = await checkpointTask;

if (checkpoint.WorkflowId != "workflow-checkpoint-validation" ||
    checkpoint.Progress.RunningStep != "checkpoint-build" ||
    checkpointResult.State != WorkflowState.Completed)
{
    Console.Error.WriteLine("Workflow P4 checkpoint validation failed.");
    Environment.ExitCode = 1;
    return;
}

var metrics = p3Runner.GetMetrics();
if (metrics.ExecutionTime <= TimeSpan.Zero ||
    metrics.CompletionRate <= 0 ||
    metrics.AverageDuration <= TimeSpan.Zero)
{
    Console.Error.WriteLine("Workflow P4 metrics validation failed.");
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Workflow P4 validation passed.");
Console.WriteLine($"P4 template: {registeredTemplate.Name}");
Console.WriteLine($"P4 checkpoint workflow: {checkpoint.WorkflowId}");
Console.WriteLine($"P4 metrics completion rate: {metrics.CompletionRate}");

var codingResult = await pipeline.ExecuteAsync(new TaskRequest
{
    TaskType = "Coding",
    Input = Directory.GetCurrentDirectory()
});

if (!codingResult.Succeeded ||
    !codingResult.Output.Contains("Repository Name:", StringComparison.OrdinalIgnoreCase) ||
    !codingResult.Output.Contains("Projects:", StringComparison.OrdinalIgnoreCase) ||
    !codingResult.Output.Contains("Languages:", StringComparison.OrdinalIgnoreCase) ||
    !codingResult.Output.Contains("Configurations:", StringComparison.OrdinalIgnoreCase) ||
    !codingResult.Output.Contains("Dependency Summary:", StringComparison.OrdinalIgnoreCase) ||
    !codingResult.Output.Contains("Repository Tree:", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Coding Agent repository intelligence validation failed.");
    Console.Error.WriteLine(codingResult.ErrorMessage);
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine("Coding Agent repository intelligence validation passed.");
Console.WriteLine(codingResult.Output);

static string DescribePayload(object? payload)
{
    return payload switch
    {
        WorkflowStep step => step.Id,
        TaskResult result => result.Output,
        Jarvis.Core.Framework.Workflow.WorkflowResult result => result.State.ToString(),
        Workflow workflow => workflow.Id,
        _ => payload?.GetType().Name ?? "none"
    };
}

static Workflow CreateSingleStepWorkflow(string workflowId, string stepId, int delayMs)
{
    return new Workflow
    {
        Id = workflowId,
        Steps =
        [
            new WorkflowStep
            {
                Id = stepId,
                Order = 1,
                TaskType = "Build",
                Input = stepId,
                Parameters = { ["DelayMs"] = delayMs }
            }
        ]
    };
}

static async Task WaitUntilAsync(Func<bool> condition)
{
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    while (!condition())
    {
        timeout.Token.ThrowIfCancellationRequested();
        await Task.Delay(25, timeout.Token);
    }
}

internal sealed class TransientFailurePipeline : ITaskPipeline
{
    private readonly ITaskPipeline inner;
    private readonly HashSet<string> failedKeys = new(StringComparer.OrdinalIgnoreCase);

    public TransientFailurePipeline(ITaskPipeline inner)
    {
        this.inner = inner;
    }

    public Task<TaskResult> ExecuteAsync(TaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Parameters.TryGetValue("FailOnceKey", out var value) &&
            value is string key &&
            failedKeys.Add(key))
        {
            return Task.FromResult(new TaskResult
            {
                RequestId = request.RequestId,
                Succeeded = false,
                ErrorMessage = "Temporary tool failure: demo transient failure."
            });
        }

        return inner.ExecuteAsync(request, cancellationToken);
    }
}

internal sealed class ValidationPipeline : ITaskPipeline
{
    public async Task<TaskResult> ExecuteAsync(TaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Parameters.TryGetValue("DelayMs", out var value) &&
            value is int delayMs &&
            delayMs > 0)
        {
            await Task.Delay(delayMs, cancellationToken);
        }

        return new TaskResult
        {
            RequestId = request.RequestId,
            Succeeded = true,
            AgentName = "Validation",
            Output = request.Input
        };
    }
}
