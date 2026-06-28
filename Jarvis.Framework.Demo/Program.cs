using Jarvis.Core.Agents.Echo;
using Jarvis.Core.Framework.Agents;
using Jarvis.Core.Framework.Context;
using Jarvis.Core.Framework.Models;
using Jarvis.Core.Framework.Planner;
using Jarvis.Core.Framework.Registry;
using Jarvis.Core.Framework.Routing;

var registry = new AgentRegistry();
registry.Register(new EchoAgent());

var planner = new TaskPlanner();
var contextManager = new ContextManager();
var toolExecutor = new ToolExecutor();
var agentManager = new AgentManager(planner, registry, contextManager, toolExecutor);
var pipeline = new TaskPipeline(agentManager);

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
