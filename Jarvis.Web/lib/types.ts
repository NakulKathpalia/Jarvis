export type ViewKey =
  | "chat"
  | "voice"
  | "memory"
  | "control"
  | "files"
  | "tools"
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
  confidence: number;
  source: string;
  memoryType: "TemporaryContext" | "SuggestedMemory" | "PermanentMemory";
  reviewStatus: "Pending" | "Approved" | "Rejected";
  expiresAtUtc?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type MemoryFormValues = {
  text: string;
  category: string;
  tags: string[];
  importance: number;
  confidence: number;
  source: string;
  memoryType: MemoryItem["memoryType"];
  reviewStatus?: MemoryItem["reviewStatus"] | null;
  expiresAtUtc?: string | null;
};

export type IngestionStatus =
  | "Uploaded"
  | "Extracted"
  | "OcrRequired"
  | "ExtractionFailed"
  | "CandidatesGenerated"
  | "Completed";

export type IngestionSourceType = "Pdf" | "Image";

export type IngestionTextBlock = {
  id: string;
  pageNumber?: number | null;
  text: string;
  confidence: number;
  status: string;
};

export type IngestionMemoryCandidate = {
  id: string;
  userId: string;
  content: string;
  suggestedCategory: string;
  suggestedMemoryType: MemoryItem["memoryType"];
  suggestedImportance: number;
  suggestedConfidence: number;
  sourceFile: string;
  sourcePage?: number | null;
  sourceTextRange: string;
  reviewStatus: MemoryItem["reviewStatus"];
  approvedMemoryId?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type IngestionJob = {
  id: string;
  userId: string;
  fileName: string;
  fileType: string;
  storedPath: string;
  status: IngestionStatus;
  extractedText: string;
  textBlocks: IngestionTextBlock[];
  createdAtUtc: string;
  updatedAtUtc: string;
  errorMessage: string;
  sourceType: IngestionSourceType;
  candidates: IngestionMemoryCandidate[];
  suggestedMemoryIds: string[];
};

export type IngestionCandidateUpdate = {
  content?: string;
  category?: string;
  memoryType?: MemoryItem["memoryType"];
  importance?: number;
  confidence?: number;
};

export type IngestionCandidateResult = {
  job: IngestionJob;
  candidate: IngestionMemoryCandidate;
  memory?: MemoryItem;
  memories?: MemoryItem[];
};

export type AppSettings = {
  ollamaBaseUrl: string;
  model: string;
  ollamaContextLength: number;
  systemPrompt: string;
  maxHistoryMessages: number;
  memoryRetrievalEnabled: boolean;
  maxRetrievedMemories: number;
  useTemporaryContext: boolean;
  useSuggestedMemories: boolean;
  fileIndexRoot: string;
  voiceMode: "PushToTalk" | "WakeWord" | "AlwaysListening" | "Hybrid";
  autoExecuteCommands: boolean;
  voiceLanguage: string;
  noiseSuppression: boolean;
  whisperExecutablePath: string;
  whisperModelPath: string;
  whisperLanguage: string;
  piperExecutablePath: string;
  piperModelPath: string;
  tesseractExecutablePath: string;
  tesseractLanguage: string;
  enableVoiceResponses: boolean;
  voiceName: string;
  speechRate: number;
  speechVolume: number;
  autoSpeakAssistantReplies: boolean;
  autoSpeakResponses: boolean;
  responseStyle: "Jarvis" | "Neutral";
  preferredLanguage: "Auto" | "English" | "RomanHinglish";
  verbosity: "Short" | "Balanced" | "Detailed";
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
  mode: "PushToTalk" | "WakeWord" | "AlwaysListening" | "Hybrid";
  implementedMode: "PushToTalk";
  autoExecuteCommands: boolean;
  voiceLanguage: string;
  noiseSuppression: boolean;
  whisper: {
    configured: boolean;
    message: string;
    whisperExecutablePath: string;
    whisperModelPath: string;
    whisperLanguage: string;
    voiceLanguage: string;
    engine: string;
    preferredDevice: string;
    fallbackDevice: string;
  };
  piper: {
    configured: boolean;
    message: string;
    piperExecutablePath: string;
    piperModelPath: string;
    autoSpeakResponses: boolean;
  };
  ocr: {
    available: boolean;
    configured: boolean;
    status: string;
    message: string;
    executablePath: string;
    language: string;
    hindiAvailable: boolean;
    installedLanguages: string[];
    tesseractExecutablePath: string;
    tesseractLanguage: string;
  };
  tts: {
    enabled: boolean;
    available: boolean;
    provider: string;
    voiceName: string;
    speechRate: number;
    speechVolume: number;
    autoSpeakAssistantReplies: boolean;
    message: string;
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
  | "Processing"
  | "Transcribing"
  | "Understanding"
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
  voiceSessionId: string;
  startedAtUtc?: string | null;
  endedAtUtc?: string | null;
  audioSizeBytes: number;
  recordingDurationMs: number;
  processingDurationMs: number;
  sttDurationMs: number;
  commandDurationMs: number;
  speechDurationMs: number;
  failureReason: string;
  lastCompletedStage: string;
  sttDevice: string;
  spokenResponse: string;
  ttsProvider: string;
  voiceUsed: string;
  playbackReady: boolean;
  playbackFailureReason: string;
  commandExecuted: boolean;
};

export type VoicePipelineStatus = {
  state: VoicePipelineState;
  updatedAtUtc: string;
  lastTranscript: string;
  lastAiResponse: string;
  message: string;
  voiceSessionId: string;
  startedAtUtc?: string | null;
  endedAtUtc?: string | null;
  audioSizeBytes: number;
  recordingDurationMs: number;
  processingDurationMs: number;
  sttDurationMs: number;
  commandDurationMs: number;
  speechDurationMs: number;
  commandDetected: boolean;
  commandExecuted: boolean;
  commandName: string;
  errorDetails: string;
  lastCompletedStage: string;
  microphoneStatus: string;
  sttDevice: string;
  spokenResponse: string;
  ttsProvider: string;
  voiceUsed: string;
  playbackReady: boolean;
  playbackFailureReason: string;
};

export type VoiceHistoryItem = {
  id: string;
  timestampUtc: string;
  transcript: string;
  response: string;
  command: string;
  assistantResponse: string;
  spokenResponse: string;
  state: VoicePipelineState;
  success: boolean;
  commandDetected: boolean;
  commandExecuted: boolean;
  processingDurationMs: number;
  sttDurationMs: number;
  commandDurationMs: number;
  speechDurationMs: number;
  ttsProvider: string;
  voiceUsed: string;
  failureReason: string;
};

export type VoiceHealthResult = {
  microphone: { status: string; message: string };
  audioCapture: { status: string; message: string };
  whisper: { available: boolean; message: string; preferredDevice: string; fallbackDevice: string; mode: string };
  ollama: { available: boolean; message: string };
  tts: { available: boolean; provider: string; voiceName: string; playbackCapability: string; message: string };
  ocr: {
    available: boolean;
    status: string;
    message: string;
    executablePath: string;
    language: string;
    hindiAvailable: boolean;
    installedLanguages: string[];
  };
  voiceService: {
    status: VoicePipelineState;
    message: string;
    lastCompletedStage: string;
    errorDetails: string;
    audioDuration: number;
    playbackSuccess: boolean;
    playbackFailureReason: string;
  };
};

export type TextToSpeechResult = {
  audioUrl: string;
  ready: boolean;
  succeeded: boolean;
  message: string;
  provider: string;
  voiceName: string;
  spokenText: string;
  speechDurationMs: number;
  failureReason: string;
};

export type VoiceDiagnosticsResult = {
  status: VoicePipelineStatus;
  recentHistory: VoiceHistoryItem[];
  recentLogs: InteractionLogEntry[];
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
