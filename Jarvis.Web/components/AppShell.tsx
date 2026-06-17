"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { ChatPanel } from "./ChatPanel";
import { ControlPanel } from "./ControlPanel";
import { FilesPanel } from "./FilesPanel";
import { MemoryPanel } from "./MemoryPanel";
import { SettingsPanel } from "./SettingsPanel";
import { Sidebar } from "./Sidebar";
import { Toast } from "./Toast";
import type { PendingVoiceCommand } from "./VoiceConfirmationCard";
import { jarvisApi } from "@/lib/api";
import type {
  AppSettings,
  ChatMessage,
  JarvisStatus,
  MemoryFormValues,
  MemoryItem,
  ViewKey
} from "@/lib/types";

export function AppShell() {
  const [activeView, setActiveView] = useState<ViewKey>("chat");
  const [status, setStatus] = useState<JarvisStatus | null>(null);
  const [history, setHistory] = useState<ChatMessage[]>([]);
  const [memory, setMemory] = useState<MemoryItem[]>([]);
  const [settings, setSettings] = useState<AppSettings | null>(null);
  const [pendingVoiceCommand, setPendingVoiceCommand] = useState<PendingVoiceCommand | null>(null);
  const [toast, setToast] = useState("");
  const [isBusy, setIsBusy] = useState(false);

  const showToast = useCallback((message: string) => {
    setToast(message);
    window.setTimeout(() => setToast(""), 2400);
  }, []);

  const refreshStatus = useCallback(async () => {
    setStatus(await jarvisApi.status());
  }, []);

  const refreshHistory = useCallback(async () => {
    setHistory(await jarvisApi.history());
  }, []);

  const refreshMemory = useCallback(async () => {
    setMemory(await jarvisApi.memory());
  }, []);

  const refreshSettings = useCallback(async () => {
    setSettings(await jarvisApi.settings());
  }, []);

  const refreshAll = useCallback(async () => {
    await Promise.all([refreshStatus(), refreshHistory(), refreshMemory(), refreshSettings()]);
  }, [refreshHistory, refreshMemory, refreshSettings, refreshStatus]);

  useEffect(() => {
    refreshAll().catch((error: Error) => showToast(error.message));
  }, [refreshAll, showToast]);

  const visibleHistory = useMemo(
    () => history.filter((message) => message.role !== "system"),
    [history]
  );

  async function sendMessage(message: string, options?: { skipAutoSpeak?: boolean }) {
    const optimisticHistory: ChatMessage[] = [
      ...history,
      { role: "user", content: message },
      { role: "assistant", content: "Thinking..." }
    ];
    setHistory(optimisticHistory);
    setIsBusy(true);

    try {
      const result = await jarvisApi.chat(message);
      const assistantResponse = result.response || "(empty response)";
      setHistory([
        ...history,
        { role: "user", content: message },
        { role: "assistant", content: assistantResponse }
      ]);
      await refreshStatus();
      if (!options?.skipAutoSpeak && settings?.autoSpeakResponses && assistantResponse !== "(empty response)") {
        await speakText(assistantResponse);
      }
      return assistantResponse;
    } catch (error) {
      showToast(error instanceof Error ? error.message : "Chat failed");
      await refreshHistory();
      return "";
    } finally {
      setIsBusy(false);
    }
  }

  async function handleVoiceCommand(transcript: string, options?: { skipAutoSpeak?: boolean }) {
    try {
      const commandResult = await jarvisApi.voiceCommand(transcript);

      if (commandResult.handled) {
        setPendingVoiceCommand(null);
        await Promise.all([refreshHistory(), refreshStatus(), refreshMemory(), refreshSettings()]);
        if (!options?.skipAutoSpeak && settings?.autoSpeakResponses) {
          await speakText(commandResult.message);
        }
        return commandResult.message;
      }

      if (commandResult.requiresConfirmation) {
        const pendingCommand: PendingVoiceCommand = {
          transcript,
          command: commandResult.command,
          message: commandResult.message,
          confirmationValue: commandResult.confirmationValue
        };
        setPendingVoiceCommand(pendingCommand);
        const message = `${commandResult.message} Use Confirm to run it, or Cancel to ignore it.`;
        setHistory([
          ...history,
          { role: "user", content: transcript },
          { role: "assistant", content: message }
        ]);
        showToast("Voice command needs confirmation");
        return message;
      }
    } catch (error) {
      showToast(error instanceof Error ? error.message : "Voice command failed");
    }

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
      setHistory((current) => [...current, { role: "assistant", content: message }]);
      await Promise.all([refreshHistory(), refreshStatus(), refreshMemory(), refreshSettings()]);
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

  function cancelVoiceCommand() {
    if (!pendingVoiceCommand) {
      return;
    }

    setPendingVoiceCommand(null);
    setHistory((current) => [...current, { role: "assistant", content: "Voice command cancelled." }]);
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
    <div className="app-frame">
      <Sidebar
        activeView={activeView}
        onChangeView={setActiveView}
        status={status}
        memoryCount={memory.length}
      />

      <main className="main-surface">
        {activeView === "chat" && (
          <ChatPanel
            autoSpeak={settings?.autoSpeakResponses ?? false}
            messages={visibleHistory}
            isBusy={isBusy}
            pendingVoiceCommand={pendingVoiceCommand}
            onRefresh={refreshAll}
            onSend={sendMessage}
            onVoiceCommand={handleVoiceCommand}
            onConfirmVoiceCommand={confirmVoiceCommand}
            onCancelVoiceCommand={cancelVoiceCommand}
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

        {activeView === "settings" && settings && (
          <SettingsPanel settings={settings} onSave={saveSettings} />
        )}
      </main>

      <Toast message={toast} />
    </div>
  );
}
