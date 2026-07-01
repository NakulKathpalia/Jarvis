namespace Jarvis.Core.Agents.Coding.Runnable;

using System.Diagnostics;
using Jarvis.Core.Agents.Coding.FileGeneration;
using Jarvis.Core.Agents.Coding.Runtime;
using Jarvis.Core.Agents.Coding.Workspace;

/// <summary>
/// Executes runnable coding tasks in safe workspaces.
/// </summary>
public sealed class RunnableExecutor
{
    private readonly RunnableTaskDetector detector;
    private readonly WorkspaceManager workspaceManager;
    private readonly FileGenerationEngine generationEngine;
    private readonly ProjectScaffolder scaffolder;
    private readonly LocalServerManager serverManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunnableExecutor"/> class.
    /// </summary>
    public RunnableExecutor()
        : this(new RunnableTaskDetector(), new WorkspaceManager(), new FileGenerationEngine(), new ProjectScaffolder(), new LocalServerManager())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RunnableExecutor"/> class.
    /// </summary>
    public RunnableExecutor(
        RunnableTaskDetector detector,
        WorkspaceManager workspaceManager,
        FileGenerationEngine generationEngine,
        ProjectScaffolder scaffolder,
        LocalServerManager serverManager)
    {
        this.detector = detector;
        this.workspaceManager = workspaceManager;
        this.generationEngine = generationEngine;
        this.scaffolder = scaffolder;
        this.serverManager = serverManager;
    }

    /// <summary>
    /// Executes a runnable task.
    /// </summary>
    public RunnableResult Execute(RunnableRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();
        var taskType = detector.Detect(request.Prompt);
        if (taskType == RunnableTaskType.None)
        {
            taskType = RunnableTaskType.Html;
        }

        var applyToRepository = request.ApplyToRepository && request.ExplicitApproval;
        var workspace = workspaceManager.Create(
            request.RepositoryPath,
            applyToRepository,
            new WorkspaceTemplate { Name = "Runnable UI", ProjectFolderName = "project" });
        var project = generationEngine.Generate(request.Prompt, taskType);
        var files = scaffolder.Scaffold(project, workspace);
        var running = serverManager.Start(workspace.ProjectPath, taskType);
        stopwatch.Stop();

        var result = new RunnableResult
        {
            Succeeded = running.IsRunning,
            TaskType = taskType,
            WorkspacePath = workspace.ProjectPath,
            Port = running.Port,
            Url = running.Url,
            ServerStatus = running.Status,
            ProcessId = running.ProcessId,
            StopInstructions = running.StopCommand,
            Duration = stopwatch.Elapsed
        };
        foreach (var file in files)
        {
            result.CreatedFiles.Add(new RunnableFile
            {
                RelativePath = file.RelativePath,
                FullPath = file.FullPath,
                Content = file.Content
            });
        }

        result.Logs.AddRange(running.Logs);
        if (!running.IsRunning)
        {
            result.Errors.AddRange(running.Logs);
        }

        return result;
    }
}
