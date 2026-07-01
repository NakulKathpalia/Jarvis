namespace Jarvis.Core.Agents.Coding.FileGeneration;

using Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Generates runnable project files.
/// </summary>
public sealed class FileGenerationEngine
{
    /// <summary>
    /// Generates a project for the runnable task.
    /// </summary>
    public GeneratedProject Generate(string request, RunnableTaskType taskType)
    {
        return taskType is RunnableTaskType.React
            ? GenerateReact(request)
            : GenerateHtml(request);
    }

    private static GeneratedProject GenerateHtml(string request)
    {
        var project = new GeneratedProject { ProjectType = RunnableTaskType.Html };
        project.Files.Add(new GeneratedFile { RelativePath = "index.html", Content = Html(request) });
        project.Files.Add(new GeneratedFile { RelativePath = "style.css", Content = Css() });
        project.Files.Add(new GeneratedFile { RelativePath = "script.js", Content = Js() });
        return project;
    }

    private static GeneratedProject GenerateReact(string request)
    {
        var project = new GeneratedProject { ProjectType = RunnableTaskType.React };
        project.Files.Add(new GeneratedFile { RelativePath = "package.json", Content = PackageJson() });
        project.Files.Add(new GeneratedFile { RelativePath = "index.html", Content = "<div id=\"root\"></div><script type=\"module\" src=\"/src/main.jsx\"></script>" });
        project.Files.Add(new GeneratedFile { RelativePath = "src/main.jsx", Content = "import React from 'react';\nimport { createRoot } from 'react-dom/client';\nimport './App.css';\nimport App from './App.jsx';\n\ncreateRoot(document.getElementById('root')).render(<App />);\n" });
        project.Files.Add(new GeneratedFile { RelativePath = "src/App.jsx", Content = ReactApp(request) });
        project.Files.Add(new GeneratedFile { RelativePath = "src/App.css", Content = Css() });
        return project;
    }

    private static string Html(string request)
    {
        var title = request.Contains("dashboard", StringComparison.OrdinalIgnoreCase) ? "Dashboard" :
            request.Contains("shopping", StringComparison.OrdinalIgnoreCase) ? "Shopping UI" : "Modern Login";
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{{title}}</title>
              <link rel="stylesheet" href="style.css">
            </head>
            <body>
              <main class="shell">
                <section class="panel">
                  <p class="eyebrow">Jarvis runnable project</p>
                  <h1>{{title}}</h1>
                  <p class="subtext">Generated in a reversible workspace and served locally.</p>
                  <form class="login-card">
                    <label>Email<input type="email" placeholder="you@example.com"></label>
                    <label>Password<input type="password" placeholder="Enter password"></label>
                    <button type="button" id="actionButton">Continue</button>
                  </form>
                  <p id="status" class="status">Ready.</p>
                </section>
              </main>
              <script src="script.js"></script>
            </body>
            </html>
            """;
    }

    private static string Css()
    {
        return """
            :root { font-family: Inter, Segoe UI, Arial, sans-serif; color: #172033; background: #eef3f8; }
            body { margin: 0; min-height: 100vh; }
            .shell { min-height: 100vh; display: grid; place-items: center; padding: 32px; }
            .panel { width: min(460px, 100%); background: white; border: 1px solid #d7e0ea; border-radius: 18px; padding: 32px; box-shadow: 0 24px 70px rgba(24, 39, 75, .12); }
            .eyebrow { margin: 0 0 10px; color: #4f6f91; font-size: 13px; font-weight: 700; text-transform: uppercase; }
            h1 { margin: 0; font-size: 34px; letter-spacing: 0; }
            .subtext { color: #5f6f83; line-height: 1.5; }
            .login-card { display: grid; gap: 16px; margin-top: 24px; }
            label { display: grid; gap: 8px; font-weight: 700; color: #243047; }
            input { border: 1px solid #c8d3df; border-radius: 10px; padding: 12px 14px; font-size: 15px; }
            button { border: 0; border-radius: 10px; padding: 13px 16px; background: #2457d6; color: white; font-weight: 800; cursor: pointer; }
            button:hover { background: #1d48b6; }
            .status { color: #43607d; min-height: 22px; }
            """;
    }

    private static string Js()
    {
        return "document.getElementById('actionButton')?.addEventListener('click', () => {\n  document.getElementById('status').textContent = 'Demo action clicked. No backend connected.';\n});\n";
    }

    private static string PackageJson()
    {
        return """
            {
              "scripts": { "dev": "vite --host 127.0.0.1" },
              "dependencies": { "@vitejs/plugin-react": "latest", "vite": "latest", "react": "latest", "react-dom": "latest" },
              "devDependencies": {}
            }
            """;
    }

    private static string ReactApp(string request)
    {
        var title = request.Contains("shopping", StringComparison.OrdinalIgnoreCase) ? "Shopping UI" : "Modern Login";
        return $$"""
            export default function App() {
              return (
                <main className="shell">
                  <section className="panel">
                    <p className="eyebrow">Jarvis runnable project</p>
                    <h1>{{title}}</h1>
                    <p className="subtext">Generated in a reversible workspace and served locally.</p>
                    <form className="login-card">
                      <label>Email<input type="email" placeholder="you@example.com" /></label>
                      <label>Password<input type="password" placeholder="Enter password" /></label>
                      <button type="button">Continue</button>
                    </form>
                  </section>
                </main>
              );
            }
            """;
    }
}
