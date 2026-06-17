export type ViewKey = "chat" | "memory" | "files" | "settings";

export type ChatMessage = {
  role: "system" | "user" | "assistant";
  content: string;
  createdAtUtc?: string;
};

export type MemoryItem = {
  id: string;
  text: string;
  category: string;
  createdAtUtc: string;
};

export type AppSettings = {
  ollamaBaseUrl: string;
  model: string;
  systemPrompt: string;
  maxHistoryMessages: number;
  fileIndexRoot: string;
  whisperExecutablePath: string;
  whisperModelPath: string;
  whisperLanguage: string;
  piperExecutablePath: string;
  piperModelPath: string;
  autoSpeakResponses: boolean;
  wakeWordEnabled: boolean;
  wakeWordPhrase: string;
  wakeWordDetectorPath: string;
  wakeWordModelPath: string;
};

export type JarvisStatus = {
  online: boolean;
  model: string;
  ollamaBaseUrl: string;
  memoryCount: number;
  historyCount: number;
};

export type VoiceStatus = {
  whisper: {
    configured: boolean;
    message: string;
    whisperExecutablePath: string;
    whisperModelPath: string;
    whisperLanguage: string;
  };
  piper: {
    configured: boolean;
    message: string;
    piperExecutablePath: string;
    piperModelPath: string;
    autoSpeakResponses: boolean;
  };
  wakeWord: {
    enabled: boolean;
    configured: boolean;
    mode?: string;
    message: string;
    wakeWordPhrase: string;
    wakeWordDetectorPath: string;
    wakeWordModelPath: string;
  };
};

export type VoiceCommandResult = {
  handled: boolean;
  requiresConfirmation: boolean;
  command: string;
  message: string;
  confirmationValue?: string;
  results?: string[];
};

export type VoiceCommandCatalogItem = {
  category: string;
  description: string;
  examples: string[];
  requiresConfirmation: boolean;
};

export type WakeWordCheckResult = {
  detected: boolean;
  message: string;
  transcript: string;
  phrase: string;
};
