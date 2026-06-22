import type {
  AppSettings,
  AssistantInputResponse,
  ChatMessage,
  ChatSession,
  ChatSessionSummary,
  DiagnosticsResult,
  FileSearchResult,
  InteractionLogEntry,
  InteractionSource,
  InteractionStatus,
  InteractionStatusResult,
  InteractionType,
  JarvisStatus,
  MemoryItem,
  MemoryFormValues,
  PcCommandCatalogItem,
  PcCommandExecutionResult,
  TextToSpeechResult,
  PcCommandLogEntry,
  VoiceCommandCatalogItem,
  VoiceCommandResult,
  VoiceDiagnosticsResult,
  VoiceHealthResult,
  VoiceHistoryItem,
  VoicePipelineResult,
  VoicePipelineStatus,
  WakeWordCheckResult,
  VoiceStatus
} from "./types";

export const API_BASE_URL = (
  process.env.NEXT_PUBLIC_JARVIS_API_URL ?? "http://localhost:5055"
).replace(/\/$/, "");

function apiUrl(path: string) {
  return `${API_BASE_URL}${path.startsWith("/") ? path : `/${path}`}`;
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  let response: Response;
  try {
    response = await fetch(apiUrl(path), {
      headers: {
        "Content-Type": "application/json",
        ...(options?.headers ?? {})
      },
      ...options
    });
  } catch (error) {
    if (!path.startsWith("/api/interactions")) {
      void rawLogInteraction("frontend-network-error", path, error instanceof Error ? error.message : "Network error");
    }
    throw error;
  }

  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    try {
      const body = (await response.json()) as { error?: string };
      message = body.error ?? message;
    } catch {
      // Keep the default error.
    }
    if (!path.startsWith("/api/interactions")) {
      void rawLogInteraction("frontend-api-error", path, message);
    }
    throw new Error(message);
  }

  return response.json() as Promise<T>;
}

async function rawLogInteraction(stage: string, input: string, error: string) {
  try {
    await fetch(apiUrl("/api/interactions/logs"), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        source: "System",
        type: "Error",
        stage,
        input,
        status: "Failed",
        message: "Frontend API request failed.",
        error
      })
    });
  } catch {
    // If the backend is down, the UI status banner is the useful signal.
  }
}

export const jarvisApi = {
  apiBaseUrl: API_BASE_URL,
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
    formData.append("audio", audio, `jarvis-voice-${Date.now()}${audioExtension(audio)}`);

    const response = await fetch(apiUrl("/api/voice/transcribe"), {
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
  voiceHealth: () => request<VoiceHealthResult>("/api/voice/health"),
  voiceDiagnostics: () => request<VoiceDiagnosticsResult>("/api/voice/diagnostics"),
  voicePipelineStatus: () => request<VoicePipelineStatus>("/api/voice/pipeline/status"),
  voiceHistory: () => request<VoiceHistoryItem[]>("/api/voice/history"),
  runVoicePipeline: async (audio: Blob, requireWakeWord = false) => {
    const formData = new FormData();
    formData.append("audio", audio, `jarvis-pipeline-${Date.now()}${audioExtension(audio)}`);
    formData.append("requireWakeWord", String(requireWakeWord));

    const response = await fetch(apiUrl("/api/voice/pipeline"), {
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
    request<TextToSpeechResult>("/api/voice/speak", {
      method: "POST",
      body: JSON.stringify({ text })
    }),
  stopSpeaking: () =>
    request<{ stopped: boolean; message: string }>("/api/voice/speak/stop", {
      method: "POST"
    }),
  voiceCommand: (transcript: string, confirmed = false) =>
    request<VoiceCommandResult>("/api/voice/command", {
      method: "POST",
      body: JSON.stringify({ transcript, confirmed })
    }),
  voiceCommands: () => request<VoiceCommandCatalogItem[]>("/api/voice/commands"),
  memory: () => request<MemoryItem[]>("/api/memory"),
  searchMemory: (filters: {
    query?: string;
    category?: string;
    tag?: string;
    minImportance?: number;
    minConfidence?: number;
    memoryType?: MemoryItem["memoryType"] | "";
    reviewStatus?: MemoryItem["reviewStatus"] | "";
  }) => {
    const params = new URLSearchParams();
    if (filters.query?.trim()) params.set("q", filters.query.trim());
    if (filters.category?.trim()) params.set("category", filters.category.trim());
    if (filters.tag?.trim()) params.set("tag", filters.tag.trim());
    if (filters.minImportance) params.set("minImportance", String(filters.minImportance));
    if (filters.minConfidence) params.set("minConfidence", String(filters.minConfidence));
    if (filters.memoryType) params.set("memoryType", filters.memoryType);
    if (filters.reviewStatus) params.set("reviewStatus", filters.reviewStatus);
    const suffix = params.toString() ? `?${params.toString()}` : "";
    return request<MemoryItem[]>(`/api/memory/search${suffix}`);
  },
  suggestedMemory: () => request<MemoryItem[]>("/api/memory/suggestions"),
  retrieveMemory: (query: string, maxResults = 5) =>
    request<{ memories: Array<{ memory: MemoryItem; score: number; matchedTerms: string[]; matchedCategories: string[] }> }>(
      "/api/memory/retrieve",
      {
        method: "POST",
        body: JSON.stringify({ query, maxResults })
      }
    ),
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
  approveMemory: (id: string) =>
    request<MemoryItem[]>(`/api/memory/${encodeURIComponent(id)}/approve`, { method: "POST" }),
  rejectMemory: (id: string) =>
    request<MemoryItem[]>(`/api/memory/${encodeURIComponent(id)}/reject`, { method: "POST" }),
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
    }),
  interactionLogs: (limit = 100) =>
    request<InteractionLogEntry[]>(`/api/interactions/logs?limit=${limit}`),
  interactionStatus: () => request<InteractionStatusResult>("/api/interactions/status"),
  clearInteractionLogs: () => request<{ cleared: boolean }>("/api/interactions/logs", { method: "DELETE" }),
  logInteraction: (entry: {
    source: InteractionSource;
    type: InteractionType;
    stage: string;
    input?: string;
    output?: string;
    status: InteractionStatus;
    message?: string;
    error?: string;
    metadata?: Record<string, unknown>;
  }) =>
    request<{ logged: boolean }>("/api/interactions/logs", {
      method: "POST",
      body: JSON.stringify(entry)
  })
};

function audioExtension(audio: Blob) {
  if (audio.type.includes("wav")) {
    return ".wav";
  }

  if (audio.type.includes("webm")) {
    return ".webm";
  }

  return ".wav";
}
