namespace Jarvis.Coding.Demo;

using System.Text.Json;
using Jarvis.Core.Agents.Coding.Parsing;
using Jarvis.Core.Agents.Coding.Context;
using Jarvis.Core.Agents.Coding.Build;
using Jarvis.Core.Agents.Coding.Git;
using Jarvis.Core.Agents.Coding.AI;
using Jarvis.Core.Agents.Coding.Assistant;
using Jarvis.Core.Agents.Coding.Execution;
using Jarvis.Core.Agents.Coding.Experience;
using Jarvis.Core.Agents.Coding.MultiAgent;
using Jarvis.Core.Agents.Coding.Models;
using Jarvis.Core.Agents.Coding.Patch;
using Jarvis.Core.Agents.Coding.Planner;
using Jarvis.Core.Agents.Coding.Review.Quality;
using Jarvis.Core.Agents.Coding.Search;
using Jarvis.Core.Agents.Coding.Services;

/// <summary>
/// Runs a read-only Coding Agent code intelligence validation demo.
/// </summary>
public static class CodingIntelligenceDemo
{
    /// <summary>
    /// Runs the demo.
    /// </summary>
    /// <param name="args">Optional repository root path.</param>
    /// <returns>The process exit code.</returns>
    public static async Task<int> Main(string[] args)
    {
        if (args.Contains("--runnable", StringComparer.OrdinalIgnoreCase))
        {
            var runnableRepositoryPath = args.FirstOrDefault(arg => !arg.StartsWith("--", StringComparison.Ordinal)) ??
                Directory.GetCurrentDirectory();
            return RunRunnableDemo(runnableRepositoryPath);
        }

        var repositoryPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        var reader = new RepositoryReader();
        var repository = reader.Read(repositoryPath);
        var repositoryIndex = new IndexBuilder().Build(repository);
        var parserRegistry = CreateParserRegistry();
        var symbolIndex = new SymbolIndexer(parserRegistry).Build(repositoryIndex);
        var knowledge = new RepositoryKnowledgeBuilder().Build(repositoryIndex, symbolIndex);
        var search = new SymbolSearch();
        var contextRequest = new ContextRequest
        {
            RequestText = "Add JWT Authentication",
            RepositoryPath = repository.RootPath
        };
        var contextPackage = new ContextBuilderV2().Build(contextRequest, repositoryIndex, symbolIndex, knowledge);
        var planningResult = new CodingPlanner().Plan(new PlanningRequest
        {
            RequestText = contextRequest.RequestText,
            ContextPackage = contextPackage
        });

        var voiceAgent = search.FindClass(symbolIndex, "VoiceAgent");
        var executeAsync = search.FindMethod(symbolIndex, "ExecuteAsync");
        var agentInterface = search.FindInterface(symbolIndex, "IAgent");

        Console.WriteLine($"Repository: {repository.RepositoryName}");
        Console.WriteLine($"Total Classes: {knowledge.ClassCount}");
        Console.WriteLine($"Total Methods: {knowledge.MethodCount}");
        Console.WriteLine($"Namespaces: {string.Join(", ", knowledge.Namespaces.Take(20))}");
        Console.WriteLine("Largest Classes:");
        PrintLines(knowledge.LargestClasses);
        Console.WriteLine("Largest Files:");
        PrintLines(knowledge.LargestFiles);
        Console.WriteLine($"Symbol Count: {knowledge.SymbolCount}");
        Console.WriteLine($"Find Class(\"VoiceAgent\"): {voiceAgent.Count}");
        Console.WriteLine($"Find Method(\"ExecuteAsync\"): {executeAsync.Count}");
        Console.WriteLine($"Find Interface(\"IAgent\"): {agentInterface.Count}");
        Console.WriteLine("ContextPackage:");
        Console.WriteLine($"- Relevant Projects: {contextPackage.Statistics.RelevantProjectCount}");
        Console.WriteLine($"- Relevant Files: {contextPackage.Statistics.RelevantFileCount}");
        Console.WriteLine($"- Relevant Symbols: {contextPackage.Statistics.RelevantSymbolCount}");
        Console.WriteLine("Relevant Files:");
        PrintLines(contextPackage.RelevantFiles.Select(file => $"{file.Path}:{file.StartLine}-{file.EndLine}"));
        Console.WriteLine("CodingPlan:");
        PrintLines(planningResult.Plan.Steps.Select(step => $"{step.Order}. {step.Title} [{step.Strategy}]"));
        RunExperienceDemo();
        RunTimelineDemo();
        var patchResult = RunPatchValidation();
        var buildAndGitResult = await RunBuildAndGitValidationAsync(repository.RootPath);
        var coderResult = await RunLocalCoderDemoAsync(repository.RootPath);

        if (knowledge.ClassCount == 0 ||
            knowledge.MethodCount == 0 ||
            knowledge.SymbolCount == 0 ||
            voiceAgent.Count == 0 ||
            executeAsync.Count == 0 ||
            agentInterface.Count == 0 ||
            contextPackage.Statistics.RelevantFileCount == 0 ||
            planningResult.Plan.Steps.Count == 0 ||
            !patchResult ||
            !buildAndGitResult ||
            !coderResult)
        {
            Console.Error.WriteLine("Coding intelligence validation failed.");
            return 1;
        }

        Console.WriteLine("Coding intelligence validation passed.");
        return 0;
    }

    private static LanguageParserRegistry CreateParserRegistry()
    {
        var registry = new LanguageParserRegistry();
        registry.Register(new CSharpParser());
        registry.Register(new TypeScriptParser());
        registry.Register(new JavaScriptParser());
        return registry;
    }

    private static int RunRunnableDemo(string repositoryPath)
    {
        var assistant = new CodingAssistant(new OllamaCodingModelClient());
        var result = assistant.RunAsync(new CodingAssistantRequest
        {
            RepositoryPath = repositoryPath,
            UserRequest = "create a login page",
            Mode = CodingAssistantMode.RunnablePreview
        }).GetAwaiter().GetResult();

        Console.WriteLine("Runnable UI Demo:");
        Console.WriteLine($"- Task Type: {result.RunnableResult.TaskType}");
        Console.WriteLine($"- Workspace: {result.RunnableResult.WorkspacePath}");
        Console.WriteLine($"- Port: {result.RunnableResult.Port}");
        Console.WriteLine($"- URL: {result.RunnableResult.Url}");
        Console.WriteLine($"- Server Status: {result.RunnableResult.ServerStatus}");
        Console.WriteLine($"- Process Id: {result.RunnableResult.ProcessId}");
        Console.WriteLine($"- Stop Server: {result.RunnableResult.StopInstructions}");
        Console.WriteLine("- Created Files:");
        PrintLines(result.RunnableResult.CreatedFiles.Select(file => file.FullPath));
        if (result.RunnableResult.Errors.Count > 0)
        {
            Console.WriteLine("- Errors:");
            PrintLines(result.RunnableResult.Errors);
        }

        Console.WriteLine($"- Main Repo Modified: {result.FilesChanged}");
        return result.Succeeded ? 0 : 1;
    }

    private static void PrintLines(IEnumerable<string> lines)
    {
        foreach (var line in lines.Take(10))
        {
            Console.WriteLine($"- {line}");
        }
    }

    private static bool RunPatchValidation()
    {
        var workspace = Path.Combine(Path.GetTempPath(), "jarvis-patch-demo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        var file = Path.Combine(workspace, "Demo.cs");
        var original = """
            namespace DemoApp;

            public class DemoService
            {
                public string GetValue()
                {
                    return "old";
                }
            }
            """;
        File.WriteAllText(file, original);

        var request = new PatchRequest
        {
            Operations =
            [
                new PatchOperation
                {
                    Type = PatchOperationType.Replace,
                    TargetPath = file,
                    SearchText = "return \"old\";",
                    ReplaceText = "return \"new\";"
                },
                new PatchOperation
                {
                    Type = PatchOperationType.Insert,
                    TargetPath = file,
                    StartLine = 9,
                    Text = """

                        public class InsertedClass
                        {
                        }
                        """
                },
                new PatchOperation
                {
                    Type = PatchOperationType.Rename,
                    TargetPath = file,
                    SymbolName = "DemoService",
                    NewName = "RenamedDemoService"
                }
            ]
        };

        var engine = new PatchEngine();
        var plan = engine.CreatePlan(request);
        var preview = engine.Preview(plan);
        var result = engine.Execute(request);
        var patched = File.ReadAllText(file);
        var rollback = new PatchRollback().Rollback(result.History);
        var restored = File.ReadAllText(file);
        Directory.Delete(workspace, true);

        Console.WriteLine("Patch Preview:");
        PrintLines(preview.Lines);
        Console.WriteLine($"Patch Applied: {result.Succeeded}");
        Console.WriteLine($"Patch Rollback: {rollback.Succeeded}");

        return result.Succeeded &&
            rollback.Succeeded &&
            patched.Contains("RenamedDemoService", StringComparison.Ordinal) &&
            patched.Contains("InsertedClass", StringComparison.Ordinal) &&
            patched.Contains("return \"new\";", StringComparison.Ordinal) &&
            restored == original;
    }

    private static async Task<bool> RunBuildAndGitValidationAsync(string repositoryPath)
    {
        var repository = new GitRepository { Path = repositoryPath };
        var gitEngine = new GitEngine();
        var status = await gitEngine.StatusAsync(repository);
        var diff = await gitEngine.DiffAsync(repository);
        var branch = await gitEngine.BranchAsync(repository);
        var history = await gitEngine.LogAsync(repository, 3);

        var buildEngine = new BuildEngine();
        var buildResult = await buildEngine.BuildAsync(new BuildRequest
        {
            RepositoryPath = repositoryPath,
            Configuration = new BuildConfiguration
            {
                Tool = "dotnet",
                Command = "dotnet",
                Arguments = "build Jarvis.Core\\Jarvis.Core.csproj -c Release",
                WorkingDirectory = repositoryPath
            }
        });

        Console.WriteLine("Git Status:");
        PrintLines(status.Lines.Take(10));
        Console.WriteLine($"Git Diff Length: {diff.Text.Length}");
        Console.WriteLine($"Git Branch: {branch.Current}");
        Console.WriteLine($"Git History Count: {history.Commits.Count}");
        Console.WriteLine("Build Result:");
        Console.WriteLine($"- Tool: {buildResult.Configuration.Tool}");
        Console.WriteLine($"- Succeeded: {buildResult.Succeeded}");
        Console.WriteLine($"- Errors: {buildResult.Errors.Count}");
        Console.WriteLine($"- Warnings: {buildResult.Warnings.Count}");
        Console.WriteLine($"- Duration: {buildResult.Statistics.Duration.TotalSeconds:n2}s");

        return buildResult.Succeeded &&
            buildResult.Statistics.ExitCode == 0 &&
            buildResult.Errors.Count == 0 &&
            !string.IsNullOrWhiteSpace(branch.Current);
    }

    private static async Task<bool> RunLocalCoderDemoAsync(string repositoryPath)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
        var codingSettings = LoadCodingModelSettings(repositoryPath);

        var assistant = new CodingAssistant(new OllamaCodingModelClient(
            httpClient,
            codingSettings.BaseUrl,
            codingSettings.ModelName,
            codingSettings.ContextLength));
        var suggestResult = await RunAssistantModeAsync(
            assistant,
            repositoryPath,
            "Suggest patch",
            CodingAssistantMode.SuggestPatch);
        var autonomousResult = await RunAssistantModeAsync(
            assistant,
            repositoryPath,
            "Autonomous preview",
            CodingAssistantMode.AutonomousPreview);
        var reviewResult = await RunAssistantModeAsync(
            assistant,
            repositoryPath,
            "Review only",
            CodingAssistantMode.ReviewOnly);
        var multiAgentResult = await RunAssistantModeAsync(
            assistant,
            repositoryPath,
            "Multi-agent preview",
            CodingAssistantMode.MultiAgentPreview);

        return suggestResult.RepositoryContext.ContextPackage.Statistics.RelevantFileCount > 0 &&
            !suggestResult.FilesChanged &&
            !autonomousResult.FilesChanged &&
            !reviewResult.FilesChanged &&
            !multiAgentResult.FilesChanged;
    }

    private static async Task<CodingAssistantResult> RunAssistantModeAsync(
        CodingAssistant assistant,
        string repositoryPath,
        string label,
        CodingAssistantMode mode)
    {
        var result = await assistant.RunAsync(new CodingAssistantRequest
        {
            RepositoryPath = repositoryPath,
            UserRequest = "Add a simple health check endpoint",
            Mode = mode
        });

        Console.WriteLine($"Jarvis Coder Demo - {label}:");
        Console.WriteLine($"- Provider: {result.ModelResponse.Provider}");
        Console.WriteLine($"- Model: {result.ModelResponse.ModelName}");
        Console.WriteLine($"- Prompt Size: {result.ModelResponse.OriginalPromptSize}");
        Console.WriteLine($"- Compressed Size: {result.ModelResponse.CompressedPromptSize}");
        Console.WriteLine($"- Execution Duration: {result.ModelResponse.Duration.TotalSeconds:n2}s");
        Console.WriteLine($"- Provider Health: {result.ModelResponse.ProviderHealthStatus}");
        Console.WriteLine($"- Model Health: {result.ModelResponse.ModelHealthStatus}");
        Console.WriteLine("- Validation Warnings:");
        PrintLines(result.ModelResponse.Warnings);
        Console.WriteLine($"- Context Files: {result.RepositoryContext.ContextPackage.Statistics.RelevantFileCount}");
        Console.WriteLine($"- Intelligent Context Before: {result.IntelligentContext.OriginalSize}");
        Console.WriteLine($"- Intelligent Context After: {result.IntelligentContext.CompressedSize}");
        Console.WriteLine("- Selected Files:");
        PrintLines(result.IntelligentContext.SelectedFiles.Select(file => file.Path));
        Console.WriteLine("- Selected Symbols:");
        PrintLines(result.IntelligentContext.SelectedSymbols.Select(symbol => $"{symbol.Kind} {symbol.Name}"));
        Console.WriteLine("- Review Findings:");
        PrintReviewFindings(result.ReviewResult.Findings);
        Console.WriteLine("- Multi-Agent Results:");
        PrintLines(result.MultiAgentResults.Select(item => $"{item.Role}: {item.Output}"));
        Console.WriteLine($"- Files Changed: {result.FilesChanged}");
        Console.WriteLine("Prompt Preview:");
        Console.WriteLine(result.PromptPreview);

        if (!result.Succeeded)
        {
            Console.WriteLine("Model Response:");
            Console.WriteLine($"Ollama unavailable or model not installed: {result.ErrorMessage}");
            Console.WriteLine("Suggested files: None");
            Console.WriteLine("Patch preview: None");
            return result;
        }

        Console.WriteLine("Model Response:");
        Console.WriteLine(result.ModelResponse.Text);
        Console.WriteLine("Suggested Files:");
        PrintLines(result.Suggestion.FilesAffected);
        Console.WriteLine("Patch Preview:");
        Console.WriteLine(string.IsNullOrWhiteSpace(result.ChangePreview.PatchText)
            ? "No patch block returned. No files were changed."
            : result.ChangePreview.PatchText);
        Console.WriteLine($"Requires Approval: {result.ChangePreview.RequiresApproval}");
        return result;
    }

    private static void PrintReviewFindings(IEnumerable<ReviewFinding> findings)
    {
        foreach (var finding in findings.Take(10))
        {
            Console.WriteLine($"- {finding.Severity} {finding.Category}: {finding.Message}");
        }
    }

    private static void RunExperienceDemo()
    {
        var engine = new ExperienceEngine();
        engine.Record(CreateExperience(
            "Add health check endpoint",
            success: true,
            ["Jarvis/Endpoints/EndpointBootstrapper.cs"],
            ["EndpointBootstrapper", "MapJarvisEndpoints"],
            ""));
        engine.Record(CreateExperience(
            "Add JWT Authentication",
            success: false,
            ["Jarvis/Auth/AuthenticationService.cs", "Jarvis/Endpoints/EndpointBootstrapper.cs"],
            ["AuthenticationService", "IAuthenticationProvider"],
            "Missing middleware registration"));
        engine.Record(CreateExperience(
            "Add command audit endpoint",
            success: true,
            ["Jarvis/Endpoints/EndpointBootstrapper.cs", "Jarvis.Core/Repositories/IAuditLogRepository.cs"],
            ["AuditLogger", "EndpointBootstrapper"],
            ""));

        var result = engine.Query(new ExperienceQuery
        {
            Repository = "jarvis",
            UserRequest = "Add JWT Authentication",
            MaxResults = 3
        });

        Console.WriteLine("Experience Demo:");
        Console.WriteLine("Most similar sessions:");
        PrintLines(result.SimilarSuccessfulSessions.Select(session => $"{session.UserRequest} [{session.Provider}/{session.Model}]"));
        Console.WriteLine("Recommended files:");
        PrintLines(result.RecommendedFiles);
        Console.WriteLine("Recommended symbols:");
        PrintLines(result.RecommendedSymbols);
        Console.WriteLine("Common failures:");
        PrintLines(result.CommonFailurePatterns.Select(pattern => $"{pattern.Reason} ({pattern.Count})"));
        Console.WriteLine($"Recommended strategy: {result.RecommendedStrategy}");
        Console.WriteLine($"Preferred style: {result.ProjectProfile.ToContextText()}");
    }

    private static ExperienceSession CreateExperience(
        string request,
        bool success,
        IEnumerable<string> files,
        IEnumerable<string> symbols,
        string failureReason)
    {
        var session = new ExperienceSession
        {
            UserRequest = request,
            Repository = "jarvis",
            Provider = "Ollama",
            Model = "qwen3:8b",
            Prompt = request,
            CompressedPrompt = request,
            Context = "namespace Jarvis.Endpoints; app.MapGet async Task GetRequiredService nullable?",
            PatchPreview = "Preview only",
            AppliedPatch = success ? "Applied fake patch" : string.Empty,
            Success = success,
            FailureReason = failureReason,
            UserApproval = success,
            Duration = TimeSpan.FromSeconds(success ? 12 : 8)
        };
        session.SelectedFiles.AddRange(files);
        session.SelectedSymbols.AddRange(symbols);
        session.CodingPlan.Add("Inspect endpoint registration.");
        session.CodingPlan.Add("Prepare minimal patch.");
        return session;
    }

    private static void RunTimelineDemo()
    {
        var reporter = new ExecutionReporter();
        reporter.EventEmitted += (_, executionEvent) =>
        {
            if (executionEvent.Type == ExecutionEventType.Completed)
            {
                Console.WriteLine($"✓ {executionEvent.Message}");
            }
            else if (executionEvent.Type == ExecutionEventType.Started)
            {
                Console.WriteLine($"⏳ {executionEvent.Message}");
            }
        };

        reporter.Start(ExecutionStage.RepositoryScan, "Repository Loaded");
        reporter.Complete(ExecutionStage.RepositoryScan, "Repository Loaded");
        reporter.SetProcessedCounts(10, 30);
        reporter.Start(ExecutionStage.ContextBuilding, "Context Building");
        reporter.Complete(ExecutionStage.ContextBuilding, "Context Built");
        reporter.Start(ExecutionStage.AIRequest, "AI Generating");
        reporter.SetModel("Ollama", "qwen3:8b");
        reporter.Complete(ExecutionStage.AIRequest, "AI Response Received");
        reporter.Start(ExecutionStage.Review, "Review Started");
        reporter.Complete(ExecutionStage.Review, "Review Complete");
        reporter.Complete(ExecutionStage.PatchPreview, "Patch Ready");
        reporter.Start(ExecutionStage.Build, "Build Started");
        reporter.Complete(ExecutionStage.Build, "Build Passed");
        reporter.Complete(ExecutionStage.Finished, "Finished");

        Console.WriteLine("Execution Timeline Demo:");
        Console.WriteLine($"Elapsed: {reporter.Timeline.Session.Progress.Elapsed.TotalSeconds:n2}s");
        Console.WriteLine($"Model: {reporter.Timeline.Session.Progress.CurrentModel}");
        Console.WriteLine($"Provider: {reporter.Timeline.Session.Progress.CurrentProvider}");
        Console.WriteLine($"Files Selected: {reporter.Timeline.Session.Progress.FilesProcessed}");
        Console.WriteLine($"Symbols Selected: {reporter.Timeline.Session.Progress.SymbolsProcessed}");
        Console.WriteLine($"Current Stage: {reporter.Timeline.Session.Progress.CurrentStage}");
        Console.WriteLine($"Average AI Time: {reporter.Metrics.AverageAITime.TotalMilliseconds:n0}ms");
        Console.WriteLine($"Average Build Time: {reporter.Metrics.AverageBuildTime.TotalMilliseconds:n0}ms");
    }

    private static CodingModelSettings LoadCodingModelSettings(string repositoryPath)
    {
        var settingsPath = Path.Combine(repositoryPath, "Jarvis", "appsettings.json");
        if (!File.Exists(settingsPath))
        {
            return new CodingModelSettings("http://localhost:11434", OllamaCodingModelClient.DefaultModel, 8192);
        }

        using var document = JsonDocument.Parse(File.ReadAllText(settingsPath));
        var root = document.RootElement;
        var baseUrl = ReadString(root, "ollamaBaseUrl", "http://localhost:11434");
        var modelName = ReadString(root, "preferredCodingModel", OllamaCodingModelClient.DefaultModel);
        var contextLength = root.TryGetProperty("ollamaContextLength", out var contextElement) &&
            contextElement.TryGetInt32(out var configuredContextLength)
                ? configuredContextLength
                : 8192;

        return new CodingModelSettings(baseUrl, modelName, contextLength);
    }

    private static string ReadString(JsonElement root, string propertyName, string fallback)
    {
        return root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(property.GetString())
                ? property.GetString()!
                : fallback;
    }

    private sealed record CodingModelSettings(string BaseUrl, string ModelName, int ContextLength);
}
