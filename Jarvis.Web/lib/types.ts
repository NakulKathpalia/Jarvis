export type ViewKey = "chat" | "memory" | "files" | "control" | "settings";

export type ChatMessage = {
  role: "system" | "user" | "assistant";
  content: string;
  createdAtUtc?: string;
};

export type MemoryItem = {
  id: string;
  text: string;
  category: string;
  tags: string[];
  importance: number;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type MemoryFormValues = {
  text: string;
  category: string;
  tags: string[];
  importance: number;
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

export type FileSearchResult = {
  path: string;
  fileName: string;
  relativePath: string;
  matchType: string;
  snippet?: string | null;
  sizeBytes: number;
  lastWriteTimeUtc: string;
};

export type CommandSafetyLevel = "Safe" | "ConfirmationRequired" | "Blocked";

export type CommandExecutionStatus = "PendingConfirmation" | "Completed" | "Failed" | "Blocked" | "Expired";

export type PcCommandCatalogItem = {
  command: string;
  description: string;
  safetyLevel: CommandSafetyLevel;
  examples: string[];
};

export type PcCommandLogEntry = {
  id: string;
  timestampUtc: string;
  originalInput: string;
  parsedCommand: string;
  target: string;
  safetyLevel: CommandSafetyLevel;
  status: CommandExecutionStatus;
  resultMessage: string;
};

export type PcCommandExecutionResult = {
  handled: boolean;
  requiresConfirmation: boolean;
  command: string;
  target: string;
  message: string;
  confirmationToken?: string | null;
  confirmationId?: string | null;
};
