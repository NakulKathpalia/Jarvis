"use client";

import { useState } from "react";
import { Composer } from "./Composer";
import { MessageList } from "./MessageList";
import { TopBar } from "./TopBar";
import { VoiceCommandHelpPanel } from "./VoiceCommandHelpPanel";
import { VoiceLoopPanel } from "./VoiceLoopPanel";
import { WakeWordPanel } from "./WakeWordPanel";
import { PendingVoiceCommand, VoiceConfirmationCard } from "./VoiceConfirmationCard";
import type { ChatMessage } from "@/lib/types";

type ChatPanelProps = {
  messages: ChatMessage[];
  isBusy: boolean;
  autoSpeak: boolean;
  pendingVoiceCommand: PendingVoiceCommand | null;
  onRefresh: () => Promise<void>;
  onSend: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onVoiceCommand: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onConfirmVoiceCommand: () => Promise<void>;
  onCancelVoiceCommand: () => void;
  onSpeak: (text: string) => Promise<void>;
  onToast: (message: string) => void;
};

export function ChatPanel({
  messages,
  isBusy,
  autoSpeak,
  pendingVoiceCommand,
  onRefresh,
  onSend,
  onVoiceCommand,
  onConfirmVoiceCommand,
  onCancelVoiceCommand,
  onSpeak,
  onToast
}: ChatPanelProps) {
  const [wakeSignal, setWakeSignal] = useState(0);

  function handleWakeActivated() {
    setWakeSignal((current) => current + 1);
  }

  return (
    <section className="chat-panel">
      <TopBar
        title="Jarvis"
        subtitle="Local-first chat powered by Ollama"
        action={
          <button className="soft-button" type="button" onClick={onRefresh}>
            Refresh
          </button>
        }
      />

      <VoiceLoopPanel
        autoSpeak={autoSpeak}
        disabled={isBusy}
        onSend={onVoiceCommand}
        onSpeak={onSpeak}
        onToast={onToast}
        wakeSignal={wakeSignal}
      />
      <WakeWordPanel onToast={onToast} onWakeActivated={handleWakeActivated} />
      <VoiceCommandHelpPanel onToast={onToast} />
      <VoiceConfirmationCard
        pendingCommand={pendingVoiceCommand}
        disabled={isBusy}
        onConfirm={onConfirmVoiceCommand}
        onCancel={onCancelVoiceCommand}
      />
      <MessageList messages={messages} onSpeak={onSpeak} />
      <Composer
        disabled={isBusy}
        onSend={onSend}
        onVoiceCommand={onVoiceCommand}
        onToast={onToast}
      />
    </section>
  );
}
