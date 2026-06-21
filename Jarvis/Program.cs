using Jarvis.Endpoints;
using Jarvis.Startup;

var builder = JarvisApplication.CreateBuilder(args);

if (await builder.TryRunCliAsync())
{
    return;
}

var app = builder.Build();

app.MapJarvisEndpoints();

await app.RunAsync();
