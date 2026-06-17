import type {
  AppSettings,
  ChatMessage,
  JarvisStatus,
  MemoryItem,
  VoiceCommandCatalogItem,
  VoiceCommandResult,
  WakeWordCheckResult,
  VoiceStatus
} from "./types";

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    headers: {
      "Content-Type": "application/json",
      ...(options?.headers ?? {})
    },
    ...options
  });

  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    try {
      const body = (await response.json()) as { error?: string };
      message = body.error ?? message;
    } catch {
      // Keep the default error.
    }
    throw new Error(message);
  }

  return response.json() as Promise<T>;
}

export const jarvisApi = {
  status: () => request<JarvisStatus>("/api/status"),
  history: () => request<ChatMessage[]>("/api/history"),
  chat: (message: string) =>
    request<{ response: string }>("/api/chat", {
      method: "POST",
      body: JSON.stringify({ message })
    }),
  transcribeVoice: async (audio: Blob) => {
    const formData = new FormData();
    formData.append("audio", audio, `jarvis-voice-${Date.now()}.webm`);

    const response = await fetch("/api/voice/transcribe", {
      method: "POST",
      body: formData
    });

    if (!response.ok) {
      let message = `Voice request failed (${response.status})`;
      try {
        const body = (await response.json()) as { error?: string };
        message = body.error ?? message;
      } catch {
        // Keep the default error.
      }
      throw new Error(message);
    }

    return response.json() as Promise<{
      transcript: string;
      ready: boolean;
      succeeded: boolean;
      fileName: string;
      contentType: string;
      sizeBytes: number;
      message: string;
    }>;
  },
  voiceStatus: () => request<VoiceStatus>("/api/voice/status"),
  wakeStatus: () =>
    request<VoiceStatus["wakeWord"]>("/api/voice/wake-status"),
  wakeCheck: (transcript: string) =>
    request<WakeWordCheckResult>("/api/voice/wake-check", {
      method: "POST",
      body: JSON.stringify({ transcript })
    }),
  speak: (text: string) =>
    request<{ audioUrl: string; ready: boolean; succeeded: boolean; message: string }>("/api/voice/speak", {
      method: "POST",
      body: JSON.stringify({ text })
    }),
  voiceCommand: (transcript: string, confirmed = false) =>
    request<VoiceCommandResult>("/api/voice/command", {
      method: "POST",
      body: JSON.stringify({ transcript, confirmed })
    }),
  voiceCommands: () => request<VoiceCommandCatalogItem[]>("/api/voice/commands"),
  memory: () => request<MemoryItem[]>("/api/memory"),
  addMemory: (text: string, category = "General") =>
    request<MemoryItem[]>("/api/memory", {
      method: "POST",
      body: JSON.stringify({ text, category })
    }),
  clearMemory: () => request<MemoryItem[]>("/api/memory", { method: "DELETE" }),
  settings: () => request<AppSettings>("/api/settings"),
  saveSettings: (settings: AppSettings) =>
    request<AppSettings>("/api/settings", {
      method: "POST",
      body: JSON.stringify(settings)
    }),
  indexFiles: () => request<{ count: number }>("/api/files/index", { method: "POST" }),
  searchFiles: (query: string) =>
    request<string[]>(`/api/files/search?q=${encodeURIComponent(query)}`)
};
