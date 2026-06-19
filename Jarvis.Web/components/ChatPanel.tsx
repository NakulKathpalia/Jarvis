"use client";

import { useEffect, useRef, useState } from "react";
import { Composer } from "./Composer";
import { MessageList } from "./MessageList";
import { VoiceCommandHelpPanel } from "./VoiceCommandHelpPanel";
import { VoiceLoopPanel } from "./VoiceLoopPanel";
import { WakeWordPanel } from "./WakeWordPanel";
import { PendingVoiceCommand, VoiceConfirmationCard } from "./VoiceConfirmationCard";
import { AssistantOrb } from "./ui/AssistantOrb";
import { AssistantActionCard } from "./ui/AssistantActionCard";
import { ChatCard } from "./ui/ChatCard";
import { PromptChip } from "./ui/PromptChip";
import { StatusBadge } from "./ui/StatusBadge";
import type { AssistantInputResponse, ChatMessage, ChatSessionSummary, JarvisStatus } from "@/lib/types";

type PendingAssistantCommand = {
  confirmationId: string;
  command: string;
  target: string;
  message: string;
};

type AssistantActivity = "idle" | "thinking" | "executing" | "speaking" | "error";

type ChatPanelProps = {
  messages: ChatMessage[];
  chats: ChatSessionSummary[];
  activeChatId: string | null;
  status: JarvisStatus | null;
  isBusy: boolean;
  autoSpeak: boolean;
  pendingVoiceCommand: PendingVoiceCommand | null;
  pendingAssistantCommand: PendingAssistantCommand | null;
  lastAssistantAction: AssistantInputResponse | null;
  assistantActivity: AssistantActivity;
  onRefresh: () => Promise<void>;
  onNewChat: () => Promise<void>;
  onOpenChat: (id: string) => Promise<void>;
  onDeleteChat: (id: string) => Promise<void>;
  onSend: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onVoiceCommand: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onConfirmVoiceCommand: () => Promise<void>;
  onCancelVoiceCommand: () => void;
  onConfirmAssistantCommand: () => Promise<void>;
  onCancelAssistantCommand: () => void;
  onSpeak: (text: string) => Promise<void>;
  onToast: (message: string) => void;
};

const promptChips = [
  "Open YouTube",
  "Open Downloads",
  "Take Screenshot",
  "Search Files",
  "Diagnostics",
  "Voice Status"
];

export function ChatPanel({
  messages,
  chats,
  activeChatId,
  status,
  isBusy,
  pendingAssistantCommand,
  lastAssistantAction,
  assistantActivity,
  pendingVoiceCommand,
  onRefresh,
  onNewChat,
  onOpenChat,
  onDeleteChat,
  onSend,
  onVoiceCommand,
  onConfirmVoiceCommand,
  onCancelVoiceCommand,
  onConfirmAssistantCommand,
  onCancelAssistantCommand,
  onSpeak,
  onToast
}: ChatPanelProps) {
  const [wakeSignal, setWakeSignal] = useState(0);
  const scrollRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    scrollRef.current?.scrollIntoView({ behavior: "smooth", block: "end" });
  }, [messages.length, isBusy]);

  function handleWakeActivated() {
    setWakeSignal((current) => current + 1);
  }

  return (
    <section className="flex min-h-screen flex-col">
      <header className="border-b border-jarvis-border/70 px-4 py-5 sm:px-6 lg:px-8">
        <div className="mx-auto flex max-w-6xl flex-col gap-5 lg:flex-row lg:items-center lg:justify-between">
          <div className="flex items-center gap-4">
            <AssistantOrb size="md" active={isBusy || status?.online} state={assistantActivity} />
            <div>
              <div className="flex flex-wrap items-center gap-2">
                <h2 className="text-2xl font-black text-jarvis-text sm:text-3xl">Jarvis Assistant</h2>
                <StatusBadge tone={status?.online ? "green" : "amber"}>
                  {status?.online ? "Ollama online" : "Offline"}
                </StatusBadge>
              </div>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-jarvis-muted">
                Local-first chat, voice, files, memory, and PC control in one private workspace.
              </p>
            </div>
          </div>
          <div className="flex flex-wrap gap-2">
            <StatusBadge>{status?.model ?? "No model"}</StatusBadge>
            <button className="soft-button" type="button" onClick={onRefresh}>Refresh</button>
            <button className="primary-button" type="button" onClick={() => void onNewChat()}>New Chat</button>
          </div>
        </div>
      </header>

      <div className="mx-auto grid w-full max-w-6xl gap-4 px-4 py-5 sm:px-6 lg:px-8">
        <VoiceLoopPanel
          disabled={isBusy}
          onRefresh={onRefresh}
          onToast={onToast}
          wakeSignal={wakeSignal}
        />
        <WakeWordPanel onToast={onToast} onWakeActivated={handleWakeActivated} />
        <VoiceConfirmationCard
          pendingCommand={pendingVoiceCommand}
          disabled={isBusy}
          onConfirm={onConfirmVoiceCommand}
          onCancel={onCancelVoiceCommand}
        />
      </div>

      {messages.length === 0 && (
        <div className="mx-auto w-full max-w-6xl px-4 pb-4 sm:px-6 lg:px-8">
          <div className="grid gap-6 rounded-[2rem] border border-jarvis-border bg-jarvis-panel/60 p-5 shadow-card sm:p-7">
            <div className="flex flex-wrap gap-2">
              {promptChips.map((chip) => (
                <PromptChip key={chip} label={chip} onClick={() => void handleQuickAction(chip, onSend)} />
              ))}
            </div>
            <div>
              <div className="mb-4 flex items-end justify-between gap-4">
                <div>
                  <h3 className="text-xl font-black text-jarvis-text">Conversation Library</h3>
                  <p className="mt-1 text-sm text-jarvis-muted">Pick up where you left off.</p>
                </div>
                <span className="text-sm text-jarvis-faint">{chats.length} chats</span>
              </div>
              <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
                {chats.map((chat) => (
                  <ChatCard
                    key={chat.id}
                    chat={chat}
                    active={chat.id === activeChatId}
                    onOpen={() => void onOpenChat(chat.id)}
                    onDelete={() => void onDeleteChat(chat.id)}
                  />
                ))}
                {chats.length === 0 && (
                  <div className="rounded-2xl border border-dashed border-jarvis-border p-6 text-sm text-jarvis-muted">
                    No saved chats yet. Send your first prompt and Jarvis will create one.
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="min-h-0 flex-1 overflow-auto">
        <MessageList
          messages={messages}
          isBusy={isBusy}
          hiddenAssistantMessages={hiddenAssistantMessages(pendingAssistantCommand, lastAssistantAction)}
          onSpeak={onSpeak}
        />
        {pendingAssistantCommand && (
          <AssistantActionCard
            title={formatCommandTitle(pendingAssistantCommand.command, pendingAssistantCommand.target)}
            message={confirmationCopy(pendingAssistantCommand)}
            state="confirm"
            command={pendingAssistantCommand.command}
            target={pendingAssistantCommand.target}
            disabled={isBusy}
            onConfirm={() => void onConfirmAssistantCommand()}
            onCancel={onCancelAssistantCommand}
          />
        )}
        {!pendingAssistantCommand && lastAssistantAction?.type === "command" && (
          <AssistantActionCard
            title={formatResultTitle(lastAssistantAction.command)}
            message={lastAssistantAction.message}
            state={lastAssistantAction.handled ? "result" : "error"}
            command={lastAssistantAction.command}
            target={lastAssistantAction.target}
          />
        )}
        {isBusy && messages.length > 0 && (
          <div className="mx-auto flex max-w-4xl gap-2 px-4 pb-4 text-jarvis-green2">
            <span className="h-2 w-2 animate-thinking-dot rounded-full bg-jarvis-green" />
            <span className="h-2 w-2 animate-thinking-dot rounded-full bg-jarvis-green [animation-delay:120ms]" />
            <span className="h-2 w-2 animate-thinking-dot rounded-full bg-jarvis-green [animation-delay:240ms]" />
          </div>
        )}
        <div ref={scrollRef} />
      </div>

      <div className="mx-auto w-full max-w-6xl px-4 sm:px-6 lg:px-8">
        <VoiceCommandHelpPanel onToast={onToast} />
      </div>
      <Composer
        disabled={isBusy}
        onSend={onSend}
        onVoiceCommand={onVoiceCommand}
        onToast={onToast}
      />
    </section>
  );
}

function handleQuickAction(
  chip: string,
  onSend: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>
) {
  if (chip === "Search Files") {
    return onSend("search web for local files");
  }

  if (chip === "Diagnostics") {
    return onSend("check diagnostics");
  }

  if (chip === "Voice Status") {
    return onSend("voice status");
  }

  return onSend(chip);
}

function confirmationCopy(command: PendingAssistantCommand) {
  const target = command.target || command.command;
  return `${humanizeCommand(command.command)} ${target}?`;
}

function hiddenAssistantMessages(
  pendingCommand: PendingAssistantCommand | null,
  lastAction: AssistantInputResponse | null
) {
  return [
    pendingCommand ? storedConfirmationMessage(pendingCommand) : "",
    lastAction?.type === "command" ? lastAction.message : ""
  ].filter(Boolean);
}

function storedConfirmationMessage(command: PendingAssistantCommand) {
  const target = command.target || command.command;
  return `Confirm ${command.command}: ${target}`;
}

function formatCommandTitle(command: string, target: string) {
  return target ? `${humanizeCommand(command)}?` : "Confirm action?";
}

function formatResultTitle(command: string) {
  return `${humanizeCommand(command)} completed`;
}

function humanizeCommand(command: string) {
  return command
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/^./, (value) => value.toUpperCase());
}
