export type ViewKey =
  | "chat"
  | "voice"
  | "memory"
  | "control"
  | "files"
  | "auth"
  | "connectedApps"
  | "security"
  | "settings"
  | "activity"
  | "diagnostics";

export type AuthProviderType = "Local" | "Google" | "Microsoft" | "GitHub" | "Discord";

export type AuthUser = {
  id: string;
  email: string;
  name: string;
  provider: AuthProviderType;
};

export type AuthStatus = {
  isAuthenticated: boolean;
  user?: AuthUser | null;
  message: string;
};

export type AuthProviderInfo = {
  id: string;
  name: string;
  type: AuthProviderType;
  configured: boolean;
  message: string;
};

export type AuthResponse = {
  succeeded: boolean;
  message: string;
  status: AuthStatus;
};

export type ConnectedAppProvider = "Google" | "Microsoft" | "GitHub" | "Discord";
export type ConnectedAppStatus = "NotConnected" | "Connected" | "NeedsSetup";

export type ConnectedAppInfo = {
  provider: ConnectedAppProvider;
  id: string;
  name: string;
  status: ConnectedAppStatus;
  description: string;
  configured: boolean;
  capabilities: string[];
};

export type ConnectedAppConnectionResult = {
  succeeded: boolean;
  message: string;
  app?: ConnectedAppInfo | null;
};

export type ChatMessage = {
  role: "system" | "user" | "assistant";
  content: string;
  createdAtUtc?: string;
};

export type ChatSession = {
  id: string;
  title: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  messages: ChatMessage[];
};

export type ChatSessionSummary = {
  id: string;
  title: string;
  preview: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  messageCount: number;
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

export type VoicePipelineState =
  | "Idle"
  | "Listening"
  | "Recording"
  | "Transcribing"
  | "WakeWordChecking"
  | "CommandDetected"
  | "AwaitingConfirmation"
  | "ExecutingCommand"
  | "GeneratingAIResponse"
  | "Speaking"
  | "Completed"
  | "Error";

export type VoicePipelineResult = {
  transcript: string;
  wakeWordDetected: boolean;
  commandDetected: boolean;
  commandName: string;
  requiresConfirmation: boolean;
  aiResponse: string;
  audioUrl: string;
  state: VoicePipelineState;
  success: boolean;
  message: string;
  confirmationId?: string | null;
};

export type VoicePipelineStatus = {
  state: VoicePipelineState;
  updatedAtUtc: string;
  lastTranscript: string;
  lastAiResponse: string;
  message: string;
};

export type VoiceHistoryItem = {
  id: string;
  timestampUtc: string;
  transcript: string;
  response: string;
  state: VoicePipelineState;
  success: boolean;
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

export type AssistantInputResponse = {
  type: "command" | "chat";
  handled: boolean;
  requiresConfirmation: boolean;
  command: string;
  target: string;
  message: string;
  response?: string | null;
  confirmationId?: string | null;
  session?: ChatSession | null;
};

export type ServiceDiagnostic = {
  healthy: boolean;
  message: string;
};

export type DiagnosticsResult = {
  platform: string;
  appDataPath: string;
  memoryPath: string;
  logsPath: string;
  screenshotPath: string;
  generatedAudioPath: string;
  ollama: ServiceDiagnostic;
  whisper: ServiceDiagnostic;
  piper: ServiceDiagnostic;
  warnings: string[];
};

export type InteractionSource = "Chat" | "Voice" | "Control" | "System";
export type InteractionType =
  | "UserInput"
  | "VoiceRecording"
  | "Transcription"
  | "WakeWordCheck"
  | "CommandParsing"
  | "Confirmation"
  | "CommandExecution"
  | "AiFallback"
  | "AiResponse"
  | "Tts"
  | "Error"
  | "SystemStatus";
export type InteractionStatus = "Started" | "Success" | "Failed" | "Pending" | "Cancelled" | "Skipped";

export type InteractionLogEntry = {
  id: string;
  timestampUtc: string;
  source: InteractionSource;
  type: InteractionType;
  stage: string;
  input: string;
  output: string;
  status: InteractionStatus;
  message: string;
  error: string;
  metadata: Record<string, unknown>;
};

export type InteractionStatusResult = {
  backendConnected: boolean;
  lastAction?: InteractionLogEntry | null;
  lastVoiceTranscript?: InteractionLogEntry | null;
  lastCommandParsed?: InteractionLogEntry | null;
  lastError?: InteractionLogEntry | null;
};
