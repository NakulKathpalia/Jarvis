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
- PC control commands with safety confirmation and JSON logs
- Stable voice pipeline with Whisper, wake word check, command routing, AI fallback, and Piper speech
- Cross-platform path resolver, platform detection, settings validation, and diagnostics endpoint
- Next.js dashboard
- CLI mode

## Voice Pipeline

The voice pipeline runs as a single backend orchestration flow:

```text
Microphone -> Record Audio -> Whisper -> Wake Word Check -> Command Detection -> Confirmation or Execution -> AI Fallback -> Piper Speech -> Result
```

Pipeline state is explicit:

```text
Idle, Listening, Recording, Transcribing, WakeWordChecking, CommandDetected, AwaitingConfirmation, ExecutingCommand, GeneratingAIResponse, Speaking, Completed, Error
```

Voice history is stored separately from chat history in local JSON.

## Diagnostics

Use the diagnostics endpoint to inspect platform and local paths:

```text
GET http://localhost:5055/api/diagnostics
```

Diagnostics includes platform, app data path, memory path, logs path, screenshot path, generated audio path, Ollama health, Whisper health, Piper health, and settings warnings.

## PC Control Commands

Supported commands are intentionally limited to known local actions. Jarvis does not execute arbitrary shell, PowerShell, cmd, script, delete, registry, credential, format, or attack commands from user text.

Safe commands run directly:

- Browser search: `search web for best local ai models`
- Volume up: `volume up`
- Volume down: `volume down`
- Mute/unmute toggle: `mute volume`

Commands that require confirmation:

- Open app: `open chrome`
- Open website: `open youtube`
- Open folder: `open folder D:\newfolder`
- Open file: `open file D:\notes.txt`
- Take screenshot: `take screenshot`
- Sleep: `sleep computer`
- Shutdown: `shutdown computer`
- Restart: `restart computer`

Unknown system commands are blocked and logged.

Command logs are stored locally as JSON with timestamp, original input, parsed command, target, safety level, status, and result message.

## Current Limitations

- Windows PC control is implemented first. macOS/Linux can be added later through the `IPcControlService` abstraction.
- Screenshots are saved locally as `.bmp` files.
- Shutdown and restart are scheduled with a short delay after confirmation.
- The dashboard should be run with `npm run dev` during active development; production builds are for release checks.
