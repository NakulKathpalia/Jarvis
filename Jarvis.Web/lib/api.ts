import type {
  AppSettings,
  AssistantInputResponse,
  ChatMessage,
  ChatSession,
  ChatSessionSummary,
  DiagnosticsResult,
  FileSearchResult,
  JarvisStatus,
  MemoryItem,
  MemoryFormValues,
  PcCommandCatalogItem,
  PcCommandExecutionResult,
  PcCommandLogEntry,
  VoiceCommandCatalogItem,
  VoiceCommandResult,
  VoiceHistoryItem,
  VoicePipelineResult,
  VoicePipelineStatus,
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
  diagnostics: () => request<DiagnosticsResult>("/api/diagnostics"),
  history: () => request<ChatMessage[]>("/api/history"),
  chat: (message: string) =>
    request<{ response: string }>("/api/chat", {
      method: "POST",
      body: JSON.stringify({ message })
    }),
  chats: () => request<ChatSessionSummary[]>("/api/chats"),
  chatSession: (id: string) => request<ChatSession>(`/api/chats/${encodeURIComponent(id)}`),
  createChat: (title?: string) =>
    request<ChatSession>("/api/chats", {
      method: "POST",
      body: JSON.stringify({ title })
    }),
  sendChatMessage: (id: string, message: string) =>
    request<{ response: string; session: ChatSession }>(`/api/chats/${encodeURIComponent(id)}/messages`, {
      method: "POST",
      body: JSON.stringify({ message })
    }),
  assistantInput: (message: string, chatSessionId?: string | null) =>
    request<AssistantInputResponse>("/api/assistant/input", {
      method: "POST",
      body: JSON.stringify({ message, chatSessionId })
    }),
  confirmAssistantCommand: (confirmationId: string, chatSessionId?: string | null) =>
    request<AssistantInputResponse>("/api/assistant/confirm", {
      method: "POST",
      body: JSON.stringify({ confirmationId, chatSessionId })
    }),
  deleteChat: (id: string) =>
    request<ChatSessionSummary[]>(`/api/chats/${encodeURIComponent(id)}`, {
      method: "DELETE"
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
  voicePipelineStatus: () => request<VoicePipelineStatus>("/api/voice/pipeline/status"),
  voiceHistory: () => request<VoiceHistoryItem[]>("/api/voice/history"),
  runVoicePipeline: async (audio: Blob, requireWakeWord = false) => {
    const formData = new FormData();
    formData.append("audio", audio, `jarvis-pipeline-${Date.now()}.webm`);
    formData.append("requireWakeWord", String(requireWakeWord));

    const response = await fetch("/api/voice/pipeline", {
      method: "POST",
      body: formData
    });

    if (!response.ok) {
      let message = `Voice pipeline failed (${response.status})`;
      try {
        const body = (await response.json()) as { error?: string };
        message = body.error ?? message;
      } catch {
        // Keep the default error.
      }
      throw new Error(message);
    }

    return response.json() as Promise<VoicePipelineResult>;
  },
  confirmVoicePipeline: (confirmationId: string) =>
    request<VoicePipelineResult>("/api/voice/pipeline/confirm", {
      method: "POST",
      body: JSON.stringify({ confirmationId })
    }),
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
  searchMemory: (filters: { query?: string; category?: string; tag?: string; minImportance?: number }) => {
    const params = new URLSearchParams();
    if (filters.query?.trim()) params.set("q", filters.query.trim());
    if (filters.category?.trim()) params.set("category", filters.category.trim());
    if (filters.tag?.trim()) params.set("tag", filters.tag.trim());
    if (filters.minImportance) params.set("minImportance", String(filters.minImportance));
    const suffix = params.toString() ? `?${params.toString()}` : "";
    return request<MemoryItem[]>(`/api/memory/search${suffix}`);
  },
  addMemory: (values: MemoryFormValues) =>
    request<MemoryItem[]>("/api/memory", {
      method: "POST",
      body: JSON.stringify(values)
    }),
  updateMemory: (id: string, values: MemoryFormValues) =>
    request<MemoryItem[]>(`/api/memory/${encodeURIComponent(id)}`, {
      method: "PUT",
      body: JSON.stringify(values)
    }),
  deleteMemory: (id: string) =>
    request<MemoryItem[]>(`/api/memory/${encodeURIComponent(id)}`, {
      method: "DELETE"
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
    request<string[]>(`/api/files/search?q=${encodeURIComponent(query)}`),
  searchFilesDetailed: (query: string, limit = 25) =>
    request<FileSearchResult[]>(
      `/api/files/search-detailed?q=${encodeURIComponent(query)}&limit=${limit}`
    ),
  openFile: (path: string) =>
    request<{ success: boolean; message: string }>(`/api/files/open`, {
      method: "POST",
      body: JSON.stringify({ path })
    }),
  openContainingFolder: (path: string) =>
    request<{ success: boolean; message: string }>(`/api/files/open-folder`, {
      method: "POST",
      body: JSON.stringify({ path })
    }),
  commandCatalog: () => request<PcCommandCatalogItem[]>("/api/commands/catalog"),
  commandLogs: () => request<PcCommandLogEntry[]>("/api/commands/logs"),
  executeCommand: (input: string) =>
    request<PcCommandExecutionResult>("/api/commands/execute", {
      method: "POST",
      body: JSON.stringify({ input })
    }),
  confirmCommand: (confirmationId: string) =>
    request<PcCommandExecutionResult>("/api/commands/confirm", {
      method: "POST",
      body: JSON.stringify({ confirmationId })
    })
};
