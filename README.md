# Jarvis

Jarvis is a local-first personal AI assistant built with a C# ASP.NET backend, a Next.js dashboard, Ollama chat, JSON memory, local file indexing, and local voice tooling.

## Development

Start the backend:

```powershell
dotnet run --project Jarvis\Jarvis.csproj
```

Backend URL:

```text
http://localhost:5055
```

Start the web dashboard in development mode:

```powershell
cd Jarvis.Web
npm run dev
```

Dashboard URL:

```text
http://localhost:3000
```

Use production builds only when the project is ready for release:

```powershell
cd Jarvis.Web
npm run build
```

## Local Assets

Keep local models, tool binaries, generated audio, and temporary files out of git. The project is designed to stay local-first and should not depend on cloud AI APIs.

## Current MVP Areas

- Local Ollama chat
- JSON memory and chat history
- Smart memory search, tags, importance, edit, and delete
- File indexing with path and text-content search
- Whisper transcription and Piper speech APIs
- Next.js dashboard
- CLI mode
