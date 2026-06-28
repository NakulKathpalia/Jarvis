"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { ActivityLogPanel } from "./ActivityLogPanel";
import { AuthPanel } from "./AuthPanel";
import { ChatPanel } from "./ChatPanel";
import { ConnectedAppsPanel } from "./ConnectedAppsPanel";
import { ControlPanel } from "./ControlPanel";
import { DiagnosticsPanel } from "./DiagnosticsPanel";
import { FilesPanel } from "./FilesPanel";
import { JarvisDialog } from "./JarvisDialog";
import { MemoryPanel } from "./MemoryPanel";
import { SettingsPanel } from "./SettingsPanel";
import { Sidebar } from "./Sidebar";
import { SecurityPanel } from "./SecurityPanel";
import { ToolsPanel } from "./ToolsPanel";
import { Toast } from "./Toast";
import { UserMenu } from "./UserMenu";
import { VoicePanel } from "./VoicePanel";
import type { PendingVoiceCommand } from "./VoiceConfirmationCard";
import { authApi } from "@/lib/authApi";
import { jarvisApi } from "@/lib/api";
import { ThemeMode, ThemeService } from "@/lib/theme";
import type {
  AppSettings,
  AssistantInputResponse,
  AuthStatus,
  ChatMessage,
  ChatSession,
  ChatSessionSummary,
  InteractionStatusResult,
  JarvisStatus,
  MemoryFormValues,
  MemoryItem,
  ViewKey
} from "@/lib/types";

type PendingAssistantCommand = {
  confirmationId: string;
  command: string;
  target: string;
  message: string;
};

type AssistantActivity = "idle" | "thinking" | "executing" | "speaking" | "completed" | "error";

export function AppShell() {
  const [activeView, setActiveView] = useState<ViewKey>("chat");
  const [activeDialog, setActiveDialog] = useState<ViewKey | "search" | null>(null);
  const [status, setStatus] = useState<JarvisStatus | null>(null);
  const [chatSummaries, setChatSummaries] = useState<ChatSessionSummary[]>([]);
  const [activeChat, setActiveChat] = useState<ChatSession | null>(null);
  const [memory, setMemory] = useState<MemoryItem[]>([]);
  const [settings, setSettings] = useState<AppSettings | null>(null);
  const [authStatus, setAuthStatus] = useState<AuthStatus | null>(null);
  const [pendingVoiceCommand, setPendingVoiceCommand] = useState<PendingVoiceCommand | null>(null);
  const [pendingAssistantCommand, setPendingAssistantCommand] = useState<PendingAssistantCommand | null>(null);
  const [lastAssistantAction, setLastAssistantAction] = useState<AssistantInputResponse | null>(null);
  const [assistantActivity, setAssistantActivity] = useState<AssistantActivity>("idle");
  const [uploadStatus, setUploadStatus] = useState("");
  const [interactionStatus, setInteractionStatus] = useState<InteractionStatusResult | null>(null);
  const [backendError, setBackendError] = useState("");
  const [toast, setToast] = useState("");
  const [isBusy, setIsBusy] = useState(false);
  const [themeMode, setThemeMode] = useState<ThemeMode>(ThemeMode.System);
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);
  const currentSpeechRef = useRef<HTMLAudioElement | null>(null);

  const showToast = useCallback((message: string) => {
    setToast(message);
    window.setTimeout(() => setToast(""), 2400);
  }, []);

  const refreshStatus = useCallback(async () => {
    try {
      setStatus(await jarvisApi.status());
      setInteractionStatus(await jarvisApi.interactionStatus());
      setBackendError("");
    } catch (error) {
      setBackendError(error instanceof Error ? error.message : "Backend unreachable");
      throw error;
    }
  }, []);

  const refreshChats = useCallback(async () => {
    setChatSummaries(await jarvisApi.chats());
  }, []);

  const refreshMemory = useCallback(async () => {
    setMemory(await jarvisApi.memory());
  }, []);

  const refreshSettings = useCallback(async () => {
    setSettings(await jarvisApi.settings());
  }, []);

  const refreshAuth = useCallback(async () => {
    setAuthStatus(await authApi.status());
  }, []);

  const refreshAll = useCallback(async () => {
    await Promise.all([refreshStatus(), refreshChats(), refreshMemory(), refreshSettings(), refreshAuth()]);
  }, [refreshAuth, refreshChats, refreshMemory, refreshSettings, refreshStatus]);

  useEffect(() => {
    refreshAll().catch((error: Error) => showToast(error.message));
  }, [refreshAll, showToast]);

  useEffect(() => {
    const initialMode = ThemeService.getInitialMode();
    setThemeMode(initialMode);
    ThemeService.apply(initialMode);

    const media = window.matchMedia("(prefers-color-scheme: light)");
    const handleThemeChange = () => {
      if (ThemeService.getInitialMode() === ThemeMode.System) {
        ThemeService.apply(ThemeMode.System);
      }
    };

    media.addEventListener("change", handleThemeChange);
    return () => media.removeEventListener("change", handleThemeChange);
  }, []);

  const visibleMessages = useMemo(
    () => (activeChat?.messages ?? []).filter((message) => message.role !== "system"),
    [activeChat]
  );

  async function createNewChat() {
    const session = await jarvisApi.createChat();
    setActiveChat(session);
    setActiveView("chat");
    setActiveDialog(null);
    await refreshChats();
  }

  async function openChat(id: string) {
    const session = await jarvisApi.chatSession(id);
    setActiveChat(session);
    setActiveView("chat");
    setActiveDialog(null);
  }

  async function deleteChat(id: string) {
    const nextSummaries = await jarvisApi.deleteChat(id);
    setChatSummaries(nextSummaries);
    if (activeChat?.id === id) {
      setActiveChat(null);
    }
    showToast("Chat deleted");
  }

  async function ensureActiveChat() {
    if (activeChat) {
      return activeChat;
    }

    const session = await jarvisApi.createChat();
    setActiveChat(session);
    await refreshChats();
    return session;
  }

  async function sendMessage(message: string, options?: { skipAutoSpeak?: boolean }) {
    setIsBusy(true);
    const initialActivity = looksLikeDirectCommand(message) ? "executing" : "thinking";
    setAssistantActivity(initialActivity);
    setLastAssistantAction(null);
    const session = await ensureActiveChat();
    const optimisticMessages: ChatMessage[] = [
      ...session.messages,
      { role: "user", content: message },
      { role: "assistant", content: activityMessage(initialActivity) }
    ];

    setActiveChat({ ...session, messages: optimisticMessages, updatedAtUtc: new Date().toISOString() });

    try {
      const result = await jarvisApi.assistantInput(message, session.id);
      const assistantResponse = result.response || result.message || "(empty response)";
      setActiveChat(result.session ?? session);
      setLastAssistantAction(result.type === "command" && !result.requiresConfirmation ? result : null);
      setPendingAssistantCommand(result.requiresConfirmation && result.confirmationId
        ? {
            confirmationId: result.confirmationId,
            command: result.command,
            target: result.target,
            message: result.message
          }
        : null);
      setAssistantActivity(result.type === "command" ? "executing" : "thinking");
      await Promise.all([refreshStatus(), refreshChats()]);
      if (result.type === "chat"
        && !options?.skipAutoSpeak
        && settings?.enableVoiceResponses
        && settings?.autoSpeakResponses
        && assistantResponse !== "(empty response)") {
        setAssistantActivity("speaking");
        await speakText(assistantResponse);
      }
      if (!result.requiresConfirmation) {
        setAssistantActivity("completed");
      }
      return assistantResponse;
    } catch (error) {
      setAssistantActivity("error");
      showToast(error instanceof Error ? error.message : "Chat failed");
      setActiveChat(await jarvisApi.chatSession(session.id));
      return "";
    } finally {
      window.setTimeout(() => {
        setAssistantActivity((current) => current === "completed" ? "idle" : current);
      }, 700);
      setIsBusy(false);
    }
  }

  async function confirmAssistantCommand() {
    if (!pendingAssistantCommand) {
      return;
    }

    setIsBusy(true);
    setAssistantActivity("executing");
    try {
      const result = await jarvisApi.confirmAssistantCommand(
        pendingAssistantCommand.confirmationId,
        activeChat?.id
      );
      setPendingAssistantCommand(null);
      setLastAssistantAction(result);
      if (result.session) {
        setActiveChat(result.session);
      }
      await Promise.all([refreshStatus(), refreshChats()]);
      showToast(result.message);
    } catch (error) {
      setAssistantActivity("error");
      showToast(error instanceof Error ? error.message : "Command confirmation failed");
    } finally {
      setAssistantActivity("idle");
      setIsBusy(false);
    }
  }

  async function cancelAssistantCommand() {
    if (!pendingAssistantCommand) {
      return;
    }

    const session = await ensureActiveChat();
    setPendingAssistantCommand(null);
    setActiveChat({
      ...session,
      messages: [...session.messages, { role: "assistant", content: "Command cancelled." }]
    });
    showToast("Command cancelled");
  }

  async function handleVoiceCommand(transcript: string, options?: { skipAutoSpeak?: boolean }) {
    return sendMessage(transcript, options);
  }

  async function handleChatAttachments(files: File[]) {
  if (files.length === 0) {
    return;
  }

  setIsBusy(true);

  try {
    const session = await ensureActiveChat();

    setActiveChat({
      ...session,
      messages: [
        ...session.messages,
        {
          role: "user",
          content: `Uploaded ${files.length} file${files.length === 1 ? "" : "s"} for Jarvis ingestion.`
        },
        {
          role: "assistant",
          content: "Reading uploaded files..."
        }
      ],
      updatedAtUtc: new Date().toISOString()
    });

    for (const file of files) {
      setUploadStatus(`Uploading ${file.name}...`);

      const uploaded = await jarvisApi.uploadIngestionFile(file);

      setUploadStatus(`Extracting text from ${file.name}...`);

      const extracted = await jarvisApi.extractIngestionJob(uploaded.id);

      if (extracted.status === "OcrRequired" || extracted.status === "ExtractionFailed") {
        setUploadStatus(`Could not extract text from ${file.name}: ${extracted.errorMessage || extracted.status}`);
        continue;
      }

      setUploadStatus(`Generating memory candidates from ${file.name}...`);

      const candidateJob = await jarvisApi.generateIngestionCandidates(uploaded.id);

      const candidateCount = candidateJob.candidates?.length ?? 0;

      const latest = await ensureActiveChat();

      setActiveChat({
        ...latest,
        messages: [
          ...latest.messages,
          {
            role: "assistant",
            content:
              candidateCount > 0
                ? `Done. I extracted text from ${file.name} and created ${candidateCount} review candidate${candidateCount === 1 ? "" : "s"}. Open Library to approve them as Memory or save as Knowledge.`
                : `Done. I extracted text from ${file.name}, but no memory candidates were created. Open Library to review the extracted text.`
          }
        ],
        updatedAtUtc: new Date().toISOString()
      });
    }

    await Promise.all([refreshMemory(), refreshChats(), refreshStatus()]);
    setUploadStatus("Upload processing completed.");
    showToast("Upload processed. Open Library to review.");
  } catch (error) {
    const message = error instanceof Error ? error.message : "Upload failed";
    setUploadStatus(message);
    showToast(message);
  } finally {
    window.setTimeout(() => setUploadStatus(""), 3000);
    setIsBusy(false);
  }
}

  async function confirmVoiceCommand() {
    if (!pendingVoiceCommand) {
      return;
    }

    setIsBusy(true);
    try {
      const result = await jarvisApi.voiceCommand(pendingVoiceCommand.transcript, true);
      setPendingVoiceCommand(null);
      const message = result.message || "Voice command completed.";
      const session = await ensureActiveChat();
      setActiveChat({ ...session, messages: [...session.messages, { role: "assistant", content: message }] });
      await Promise.all([refreshStatus(), refreshMemory(), refreshSettings(), refreshChats()]);
      if (settings?.enableVoiceResponses && settings?.autoSpeakResponses) {
        await speakText(message);
      }
      showToast("Voice command confirmed");
    } catch (error) {
      showToast(error instanceof Error ? error.message : "Voice command confirmation failed");
    } finally {
      setIsBusy(false);
    }
  }

  async function cancelVoiceCommand() {
    if (!pendingVoiceCommand) {
      return;
    }

    setPendingVoiceCommand(null);
    const session = await ensureActiveChat();
    setActiveChat({ ...session, messages: [...session.messages, { role: "assistant", content: "Voice command cancelled." }] });
    showToast("Voice command cancelled");
  }

  async function addMemory(values: MemoryFormValues) {
    setMemory(await jarvisApi.addMemory(values));
    await refreshStatus();
    showToast("Memory saved");
  }

  async function updateMemory(id: string, values: MemoryFormValues) {
    setMemory(await jarvisApi.updateMemory(id, values));
    await refreshStatus();
    showToast("Memory updated");
  }

  async function deleteMemory(id: string) {
    setMemory(await jarvisApi.deleteMemory(id));
    await refreshStatus();
    showToast("Memory deleted");
  }

  async function approveMemory(id: string) {
    setMemory(await jarvisApi.approveMemory(id));
    await refreshStatus();
    showToast("Memory approved");
  }

  async function rejectMemory(id: string) {
    setMemory(await jarvisApi.rejectMemory(id));
    await refreshStatus();
    showToast("Memory rejected");
  }

  async function clearMemory() {
    setMemory(await jarvisApi.clearMemory());
    await refreshStatus();
    showToast("Memory cleared");
  }

  async function saveSettings(nextSettings: AppSettings) {
    setSettings(await jarvisApi.saveSettings(nextSettings));
    await refreshStatus();
    showToast("Settings saved");
  }

  function changeThemeMode(nextMode: ThemeMode) {
    setThemeMode(nextMode);
    ThemeService.apply(nextMode);
  }

  function changeWorkspace(view: ViewKey) {
    if (view === "chat") {
      setActiveView("chat");
      setActiveDialog(null);
      setMobileSidebarOpen(false);
      return;
    }

    setActiveView(view);
    setActiveDialog(view);
    setMobileSidebarOpen(false);
  }

  function openSearch() {
    setActiveDialog("search");
    setMobileSidebarOpen(false);
  }

  useEffect(() => {
    function handleShortcut(event: KeyboardEvent) {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        openSearch();
      }

      if ((event.ctrlKey || event.metaKey) && event.shiftKey && event.key.toLowerCase() === "m") {
        event.preventDefault();
        changeWorkspace("memory");
      }
    }

    window.addEventListener("keydown", handleShortcut);
    return () => window.removeEventListener("keydown", handleShortcut);
  }, []);

  async function speakText(text: string) {
    try {
      stopCurrentSpeech();
      const result = await jarvisApi.speak(text);
      if (!result.audioUrl) {
        showToast(result.message || "No audio returned");
        return;
      }

      const audio = new Audio(resolveAudioUrl(result.audioUrl));
      currentSpeechRef.current = audio;
      audio.addEventListener("ended", () => {
        if (currentSpeechRef.current === audio) {
          currentSpeechRef.current = null;
        }
      });
      await audio.play();
    } catch (error) {
      showToast(error instanceof Error ? error.message : "Speech playback failed");
    }
  }

  function stopCurrentSpeech() {
    const audio = currentSpeechRef.current;
    if (audio) {
      audio.pause();
      audio.currentTime = 0;
      currentSpeechRef.current = null;
    }
    void jarvisApi.stopSpeaking().catch(() => undefined);
  }

  function resolveAudioUrl(audioUrl: string) {
    if (audioUrl.startsWith("http://") || audioUrl.startsWith("https://")) {
      return audioUrl;
    }

    return `${jarvisApi.apiBaseUrl}${audioUrl.startsWith("/") ? audioUrl : `/${audioUrl}`}`;
  }

  function renderDialogContent() {
    switch (activeDialog) {
      case "search":
        return (
          <div className="command-palette">
            <button type="button" onClick={() => void createNewChat()}>Start a new chat</button>
            <button type="button" onClick={() => changeWorkspace("memory")}>Open Memory and Knowledge</button>
            <button type="button" onClick={() => changeWorkspace("voice")}>Open Voice controls</button>
            <button type="button" onClick={() => changeWorkspace("settings")}>Open Settings</button>
            <button type="button" onClick={() => changeWorkspace("diagnostics")}>Open Diagnostics</button>
          </div>
        );
      case "memory":
        return (
          <MemoryPanel
            items={memory}
            onAdd={addMemory}
            onUpdate={updateMemory}
            onDelete={deleteMemory}
            onApprove={approveMemory}
            onReject={rejectMemory}
            onClear={clearMemory}
            onRefresh={refreshMemory}
          />
        );
      case "files":
        return <FilesPanel onToast={showToast} />;
      case "control":
        return <ControlPanel onToast={showToast} />;
      case "voice":
        return (
          <VoicePanel
            disabled={isBusy}
            appStatus={status}
            memoryCount={memory.length}
            onRefresh={refreshAll}
            onToast={showToast}
          />
        );
      case "activity":
        return <ActivityLogPanel />;
      case "tools":
        return <ToolsPanel onChangeView={changeWorkspace} />;
      case "auth":
        return <AuthPanel onToast={showToast} onAuthChanged={refreshAuth} />;
      case "connectedApps":
        return <ConnectedAppsPanel onToast={showToast} />;
      case "settings":
        return settings ? (
          <SettingsPanel
            settings={settings}
            themeMode={themeMode}
            onThemeModeChange={changeThemeMode}
            onSave={saveSettings}
          />
        ) : <div className="dialog-empty">Settings are loading.</div>;
      case "diagnostics":
        return <DiagnosticsPanel />;
      case "security":
        return (
          <SecurityPanel
            interactionStatus={interactionStatus}
            pendingAssistantCommand={pendingAssistantCommand}
            lastAssistantAction={lastAssistantAction}
          />
        );
      case "chat":
      default:
        return null;
    }
  }

  const dialogMeta = activeDialog ? getViewMeta(activeDialog === "search" ? "tools" : activeDialog) : null;

  return (
    <div className="app-shell-v2">
      <button
        aria-label="Close sidebar"
        className={mobileSidebarOpen ? "mobile-sidebar-backdrop open" : "mobile-sidebar-backdrop"}
        type="button"
        onClick={() => setMobileSidebarOpen(false)}
      />
      <div className={mobileSidebarOpen ? "sidebar-shell open" : "sidebar-shell"}>
        <Sidebar
          activeView={activeView}
          activeChatId={activeChat?.id ?? null}
          chats={chatSummaries}
          onChangeView={changeWorkspace}
          onSearch={openSearch}
          onNewChat={createNewChat}
          onOpenChat={openChat}
          status={status}
          memoryCount={memory.length}
        />
      </div>

      <main className="jarvis-main">
        <header className="jarvis-chat-topbar">
          <button className="mobile-menu-button" type="button" onClick={() => setMobileSidebarOpen((current) => !current)} aria-label="Open sidebar">
            Menu
          </button>
          <div className="jarvis-chat-title">
            <span className={status?.online ? "dot online" : "dot"} />
            <div>
              <h2>Jarvis</h2>
              <p>{backendError || (status?.online ? "Jarvis online." : "Backend offline.")}</p>
            </div>
          </div>
          <div className="jarvis-chat-actions">
            <button type="button" onClick={() => changeWorkspace("memory")}>Library</button>
            <button type="button" onClick={() => changeWorkspace("voice")}>Voice</button>
            <button type="button" onClick={() => changeWorkspace("settings")}>Settings</button>
            <UserMenu
              authStatus={authStatus}
              onChangeView={changeWorkspace}
              onAuthChanged={refreshAuth}
              onToast={showToast}
            />
          </div>
        </header>

        <ChatPanel
          autoSpeak={settings?.autoSpeakResponses ?? false}
          messages={visibleMessages}
          chats={chatSummaries}
          activeChatId={activeChat?.id ?? null}
          status={status}
          isBusy={isBusy}
          pendingVoiceCommand={pendingVoiceCommand}
          pendingAssistantCommand={pendingAssistantCommand}
          lastAssistantAction={lastAssistantAction}
          assistantActivity={assistantActivity}
          uploadStatus={uploadStatus}
          onAttachmentsSelected={handleChatAttachments}
          onRefresh={refreshAll}
          onNewChat={createNewChat}
          onOpenChat={openChat}
          onDeleteChat={deleteChat}
          onSend={sendMessage}
          onVoiceCommand={handleVoiceCommand}
          onConfirmVoiceCommand={confirmVoiceCommand}
          onCancelVoiceCommand={() => void cancelVoiceCommand()}
          onConfirmAssistantCommand={confirmAssistantCommand}
          onCancelAssistantCommand={() => void cancelAssistantCommand()}
          onSpeak={speakText}
          onToast={showToast}
        />
      </main>

      <JarvisDialog
        open={activeDialog !== null && activeDialog !== "chat"}
        title={activeDialog === "search" ? "Search and commands" : dialogMeta?.title ?? "Jarvis"}
        subtitle={activeDialog === "search" ? "Jump anywhere in Jarvis." : dialogMeta?.subtitle}
        size={activeDialog === "search" ? "md" : "xl"}
        onClose={() => {
          setActiveDialog(null);
          setActiveView("chat");
        }}
      >
        {renderDialogContent()}
      </JarvisDialog>

      <Toast message={toast} />
    </div>
  );
}

function looksLikeDirectCommand(input: string) {
  return /^(open|launch|start|run|show|search|find|take|set|mute|unmute|volume|shutdown|restart|sleep)\b/i.test(input.trim());
}

function activityMessage(activity: AssistantActivity) {
  switch (activity) {
    case "executing":
      return "Executing...";
    case "speaking":
      return "Speaking...";
    case "completed":
      return "Completed.";
    case "error":
      return "Something went wrong.";
    case "thinking":
    case "idle":
    default:
      return "Thinking...";
  }
}

function getViewMeta(view: ViewKey) {
  switch (view) {
    case "chat":
      return { title: "Jarvis", subtitle: "Jarvis online." };
    case "tools":
      return { title: "Tools", subtitle: "Quick access to Jarvis capabilities." };
    case "voice":
      return { title: "Voice", subtitle: "Push-to-talk, diagnostics, and response playback." };
    case "memory":
      return { title: "Memory and Knowledge", subtitle: "Review facts, uploads, OCR output, and reference material." };
    case "control":
      return { title: "Commands", subtitle: "PC control remains protected by security and permissions." };
    case "files":
      return { title: "Files", subtitle: "Search local indexed files." };
    case "auth":
      return { title: "Account", subtitle: "Local auth foundation and future providers." };
    case "connectedApps":
      return { title: "Connected Apps", subtitle: "Future external app connections." };
    case "security":
      return { title: "Security", subtitle: "Command risk and audit overview." };
    case "settings":
      return { title: "Settings", subtitle: "Model, voice, memory, OCR, and appearance." };
    case "diagnostics":
      return { title: "Diagnostics", subtitle: "Runtime health, local paths, and service status." };
    case "activity":
      return { title: "Activity", subtitle: "Interaction and audit timeline." };
  }
}
