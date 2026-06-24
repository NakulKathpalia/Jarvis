"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { ActivityLogPanel } from "./ActivityLogPanel";
import { AppLayout } from "./AppLayout";
import { AuthPanel } from "./AuthPanel";
import { ChatPanel } from "./ChatPanel";
import { ConnectedAppsPanel } from "./ConnectedAppsPanel";
import { ControlPanel } from "./ControlPanel";
import { DiagnosticsPanel } from "./DiagnosticsPanel";
import { FilesPanel } from "./FilesPanel";
import { MemoryPanel } from "./MemoryPanel";
import { SettingsPanel } from "./SettingsPanel";
import { Sidebar } from "./Sidebar";
import { SecurityPanel } from "./SecurityPanel";
import { JarvisStatusPanel } from "./JarvisStatusPanel";
import { ToolsPanel } from "./ToolsPanel";
import { TopNavbar } from "./TopNavbar";
import { Toast } from "./Toast";
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
    await refreshChats();
  }

  async function openChat(id: string) {
    const session = await jarvisApi.chatSession(id);
    setActiveChat(session);
    setActiveView("chat");
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

  const viewMeta = getViewMeta(activeView);

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

  return (
    <AppLayout
      mobileSidebarOpen={mobileSidebarOpen}
      onCloseMobileSidebar={() => setMobileSidebarOpen(false)}
      sidebar={
        <Sidebar
          activeView={activeView}
          activeChatId={activeChat?.id ?? null}
          chats={chatSummaries}
          onChangeView={setActiveView}
          onNewChat={createNewChat}
          onOpenChat={openChat}
          status={status}
          memoryCount={memory.length}
        />
      }
      topbar={
        <TopNavbar
          title={viewMeta.title}
          subtitle={viewMeta.subtitle}
          authStatus={authStatus}
          onChangeView={setActiveView}
          onAuthChanged={refreshAuth}
          onToast={showToast}
          onToggleSidebar={() => setMobileSidebarOpen((current) => !current)}
        />
      }
      rightPanel={
        <JarvisStatusPanel
          status={status}
          interactionStatus={interactionStatus}
          pendingAssistantCommand={pendingAssistantCommand}
          lastAssistantAction={lastAssistantAction}
        />
      }
    >
      {activeView === "chat" && (
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
      )}

      {activeView === "memory" && (
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
      )}

      {activeView === "files" && <FilesPanel onToast={showToast} />}

      {activeView === "control" && <ControlPanel onToast={showToast} />}

      {activeView === "voice" && (
        <VoicePanel
          disabled={isBusy}
          appStatus={status}
          memoryCount={memory.length}
          onRefresh={refreshAll}
          onToast={showToast}
        />
      )}

      {activeView === "activity" && <ActivityLogPanel />}

      {activeView === "tools" && <ToolsPanel onChangeView={setActiveView} />}

      {activeView === "auth" && <AuthPanel onToast={showToast} onAuthChanged={refreshAuth} />}

      {activeView === "connectedApps" && <ConnectedAppsPanel onToast={showToast} />}

      {activeView === "settings" && settings && (
        <SettingsPanel
          settings={settings}
          themeMode={themeMode}
          onThemeModeChange={changeThemeMode}
          onSave={saveSettings}
        />
      )}

      {activeView === "diagnostics" && <DiagnosticsPanel />}

      {activeView === "security" && (
        <SecurityPanel
          interactionStatus={interactionStatus}
          pendingAssistantCommand={pendingAssistantCommand}
          lastAssistantAction={lastAssistantAction}
        />
      )}

      <Toast message={toast} />
    </AppLayout>
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
      return { title: "Tools & Settings", subtitle: "Voice, memory, files, security, account, and diagnostics" };
    case "voice":
      return { title: "Voice", subtitle: "Push-to-talk voice is ready." };
    case "memory":
      return { title: "Memory", subtitle: "Local private facts and search" };
    case "control":
      return { title: "PC Control", subtitle: "Commands checked by Jarvis Security" };
    case "files":
      return { title: "Files", subtitle: "Local file indexing and search" };
    case "auth":
      return { title: "Auth", subtitle: "Placeholder local sign in and future OAuth" };
    case "connectedApps":
      return { title: "Connected Apps", subtitle: "Future external app connections" };
    case "security":
      return { title: "Security", subtitle: "Command risk and audit overview" };
    case "settings":
      return { title: "Settings", subtitle: "Runtime configuration and appearance" };
    case "diagnostics":
      return { title: "Diagnostics", subtitle: "Runtime health and local paths" };
    case "activity":
      return { title: "Activity", subtitle: "Interaction and audit timeline" };
  }
}
