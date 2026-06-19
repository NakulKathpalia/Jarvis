"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { AppLayout } from "./AppLayout";
import { ChatPanel } from "./ChatPanel";
import { ControlPanel } from "./ControlPanel";
import { DiagnosticsPanel } from "./DiagnosticsPanel";
import { FilesPanel } from "./FilesPanel";
import { MemoryPanel } from "./MemoryPanel";
import { SettingsPanel } from "./SettingsPanel";
import { Sidebar } from "./Sidebar";
import { Toast } from "./Toast";
import { VoicePanel } from "./VoicePanel";
import type { PendingVoiceCommand } from "./VoiceConfirmationCard";
import { jarvisApi } from "@/lib/api";
import type {
  AppSettings,
  AssistantInputResponse,
  ChatMessage,
  ChatSession,
  ChatSessionSummary,
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

type AssistantActivity = "idle" | "thinking" | "executing" | "speaking" | "error";

export function AppShell() {
  const [activeView, setActiveView] = useState<ViewKey>("chat");
  const [status, setStatus] = useState<JarvisStatus | null>(null);
  const [chatSummaries, setChatSummaries] = useState<ChatSessionSummary[]>([]);
  const [activeChat, setActiveChat] = useState<ChatSession | null>(null);
  const [memory, setMemory] = useState<MemoryItem[]>([]);
  const [settings, setSettings] = useState<AppSettings | null>(null);
  const [pendingVoiceCommand, setPendingVoiceCommand] = useState<PendingVoiceCommand | null>(null);
  const [pendingAssistantCommand, setPendingAssistantCommand] = useState<PendingAssistantCommand | null>(null);
  const [lastAssistantAction, setLastAssistantAction] = useState<AssistantInputResponse | null>(null);
  const [assistantActivity, setAssistantActivity] = useState<AssistantActivity>("idle");
  const [toast, setToast] = useState("");
  const [isBusy, setIsBusy] = useState(false);

  const showToast = useCallback((message: string) => {
    setToast(message);
    window.setTimeout(() => setToast(""), 2400);
  }, []);

  const refreshStatus = useCallback(async () => {
    setStatus(await jarvisApi.status());
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

  const refreshAll = useCallback(async () => {
    await Promise.all([refreshStatus(), refreshChats(), refreshMemory(), refreshSettings()]);
  }, [refreshChats, refreshMemory, refreshSettings, refreshStatus]);

  useEffect(() => {
    refreshAll().catch((error: Error) => showToast(error.message));
  }, [refreshAll, showToast]);

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
    setAssistantActivity("thinking");
    setLastAssistantAction(null);
    const session = await ensureActiveChat();
    const optimisticMessages: ChatMessage[] = [
      ...session.messages,
      { role: "user", content: message },
      { role: "assistant", content: "Thinking..." }
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
      if (result.type === "chat" && !options?.skipAutoSpeak && settings?.autoSpeakResponses && assistantResponse !== "(empty response)") {
        setAssistantActivity("speaking");
        await speakText(assistantResponse);
      }
      return assistantResponse;
    } catch (error) {
      setAssistantActivity("error");
      showToast(error instanceof Error ? error.message : "Chat failed");
      setActiveChat(await jarvisApi.chatSession(session.id));
      return "";
    } finally {
      setAssistantActivity("idle");
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
      if (settings?.autoSpeakResponses) {
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

  async function speakText(text: string) {
    try {
      const result = await jarvisApi.speak(text);
      if (!result.audioUrl) {
        showToast(result.message || "No audio returned");
        return;
      }

      const audio = new Audio(result.audioUrl);
      await audio.play();
    } catch (error) {
      showToast(error instanceof Error ? error.message : "Speech playback failed");
    }
  }

  return (
    <AppLayout
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
          onClear={clearMemory}
        />
      )}

      {activeView === "files" && <FilesPanel onToast={showToast} />}

      {activeView === "control" && <ControlPanel onToast={showToast} />}

      {activeView === "voice" && (
        <VoicePanel disabled={isBusy} onRefresh={refreshAll} onToast={showToast} />
      )}

      {activeView === "settings" && settings && (
        <SettingsPanel settings={settings} onSave={saveSettings} />
      )}

      {activeView === "diagnostics" && <DiagnosticsPanel />}

      <Toast message={toast} />
    </AppLayout>
  );
}
