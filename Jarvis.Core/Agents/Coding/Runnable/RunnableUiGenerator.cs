namespace Jarvis.Core.Agents.Coding.Runnable;

/// <summary>
/// Generates runnable UI files.
/// </summary>
public sealed class RunnableUiGenerator
{
    /// <summary>
    /// Generates files for the requested task type.
    /// </summary>
    public IReadOnlyList<RunnableFile> Generate(string request, RunnableTaskType taskType)
    {
        return taskType == RunnableTaskType.React
            ? GenerateReact(request)
            : GenerateStatic(request);
    }

    private static IReadOnlyList<RunnableFile> GenerateStatic(string request)
    {
        return
        [
            new RunnableFile { RelativePath = "index.html", Content = BuildHtml(request) },
            new RunnableFile { RelativePath = "style.css", Content = BuildCss() },
            new RunnableFile { RelativePath = "script.js", Content = BuildScript() }
        ];
    }

    private static IReadOnlyList<RunnableFile> GenerateReact(string request)
    {
        return
        [
            new RunnableFile { RelativePath = "package.json", Content = BuildPackageJson() },
            new RunnableFile { RelativePath = "index.html", Content = "<div id=\"root\"></div><script type=\"module\" src=\"/src/main.jsx\"></script>" },
            new RunnableFile { RelativePath = "src/main.jsx", Content = "import React from 'react';\nimport { createRoot } from 'react-dom/client';\nimport './App.css';\nimport App from './App.jsx';\n\ncreateRoot(document.getElementById('root')).render(<App />);\n" },
            new RunnableFile { RelativePath = "src/App.jsx", Content = BuildReactApp(request) },
            new RunnableFile { RelativePath = "src/App.css", Content = BuildCss() }
        ];
    }

    /// <summary>
    /// Writes generated files to a workspace.
    /// </summary>
    public IReadOnlyList<RunnableFile> WriteFiles(RunnableWorkspace workspace, IEnumerable<RunnableFile> files)
    {
        var written = new List<RunnableFile>();
        foreach (var file in files)
        {
            var path = Path.Combine(workspace.RootPath, file.RelativePath);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, file.Content);
            file.FullPath = path;
            written.Add(file);
        }

        return written;
    }

    private static string BuildHtml(string request)
    {
        var title = request.Contains("shopping", StringComparison.OrdinalIgnoreCase) ? "Shopping App" : "Login Page";
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
                  <p class="eyebrow">Jarvis runnable preview</p>
                  <h1>{{title}}</h1>
                  <p class="subtext">A clean local UI generated in a safe demo workspace.</p>
                  <form class="login-card">
                    <label>Email<input type="email" placeholder="you@example.com"></label>
                    <label>Password<input type="password" placeholder="Enter password"></label>
                    <button type="button" id="loginButton">Sign in</button>
                  </form>
                  <p id="status" class="status">Ready.</p>
                </section>
              </main>
              <script src="script.js"></script>
            </body>
            </html>
            """;
    }

    private static string BuildCss()
    {
        return """
            :root { font-family: Inter, Segoe UI, Arial, sans-serif; color: #172033; background: #eef3f8; }
            body { margin: 0; min-height: 100vh; }
            .shell { min-height: 100vh; display: grid; place-items: center; padding: 32px; }
            .panel { width: min(440px, 100%); background: white; border: 1px solid #d7e0ea; border-radius: 18px; padding: 32px; box-shadow: 0 24px 70px rgba(24, 39, 75, .12); }
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

    private static string BuildScript()
    {
        return """
            document.getElementById('loginButton')?.addEventListener('click', () => {
              document.getElementById('status').textContent = 'Demo login clicked. No backend connected.';
            });
            """;
    }

    private static string BuildPackageJson()
    {
        return """
            {
              "scripts": { "dev": "vite --host 127.0.0.1" },
              "dependencies": { "@vitejs/plugin-react": "latest", "vite": "latest", "react": "latest", "react-dom": "latest" },
              "devDependencies": {}
            }
            """;
    }

    private static string BuildReactApp(string request)
    {
        var title = request.Contains("shopping", StringComparison.OrdinalIgnoreCase) ? "Shopping App" : "Login Page";
        return $$"""
            export default function App() {
              return (
                <main className="shell">
                  <section className="panel">
                    <p className="eyebrow">Jarvis runnable preview</p>
                    <h1>{{title}}</h1>
                    <p className="subtext">A Vite React UI generated in a safe demo workspace.</p>
                    <form className="login-card">
                      <label>Email<input type="email" placeholder="you@example.com" /></label>
                      <label>Password<input type="password" placeholder="Enter password" /></label>
                      <button type="button">Sign in</button>
                    </form>
                  </section>
                </main>
              );
            }
            """;
    }
}
