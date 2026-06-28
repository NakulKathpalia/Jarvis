"use client";

import { useEffect, useRef } from "react";
import { Composer } from "./Composer";
import type { AssistantInputResponse, ChatMessage, ChatSessionSummary, JarvisStatus } from "@/lib/types";

export type PendingAssistantCommand = {
  confirmationId: string;
  command: string;
  target: string;
  message: string;
};

type AssistantActivity = "idle" | "thinking" | "executing" | "speaking" | "completed" | "error";

type ChatPanelProps = {
  messages: ChatMessage[];
  chats: ChatSessionSummary[];
  activeChatId: string | null;
  status: JarvisStatus | null;
  isBusy: boolean;
  autoSpeak: boolean;
  pendingVoiceCommand: unknown;
  pendingAssistantCommand: PendingAssistantCommand | null;
  lastAssistantAction: AssistantInputResponse | null;
  assistantActivity: AssistantActivity;
  uploadStatus?: string;
  onRefresh: () => Promise<void>;
  onNewChat: () => Promise<void>;
  onOpenChat: (id: string) => Promise<void>;
  onDeleteChat: (id: string) => Promise<void>;
  onSend: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onVoiceCommand: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onAttachmentsSelected?: (files: File[]) => Promise<void>;
  onConfirmVoiceCommand: () => Promise<void>;
  onCancelVoiceCommand: () => void;
  onConfirmAssistantCommand: () => Promise<void>;
  onCancelAssistantCommand: () => void;
  onSpeak: (text: string) => Promise<void>;
  onToast: (message: string) => void;
};

const suggestions = [
  { title: "Open YouTube", example: "Open YouTube" },
  { title: "Upload PDF", example: "Upload a PDF and save useful parts as memory or knowledge" },
  { title: "Upload Image", example: "Upload an image and extract text with OCR" },
  { title: "Ask memory", example: "What do you remember about me?" }
];

export function ChatPanel({
  messages,
  isBusy,
  pendingAssistantCommand,
  lastAssistantAction,
  assistantActivity,
  uploadStatus,
  onSend,
  onVoiceCommand,
  onAttachmentsSelected,
  onConfirmAssistantCommand,
  onCancelAssistantCommand,
  onToast
}: ChatPanelProps) {
  const scrollRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    scrollRef.current?.scrollIntoView({ behavior: "smooth", block: "end" });
  }, [messages.length, isBusy, pendingAssistantCommand, uploadStatus]);

  return (
    <section className="chat-screen">
      <div className="chat-scroll">
        <div className="chat-content">
          {messages.length === 0 && (
            <div className="empty-state">
              <div className="empty-orb">J</div>
              <h3>How can I help?</h3>
              <p>Chat naturally, run a command, upload notes, or ask Jarvis what it remembers.</p>
              <div className="suggestion-grid">
                {suggestions.map((suggestion) => (
                  <button
                    className="suggestion-card"
                    key={suggestion.title}
                    type="button"
                    onClick={() => void onSend(suggestion.example)}
                  >
                    <strong>{suggestion.title}</strong>
                    <span>{suggestion.example}</span>
                  </button>
                ))}
              </div>
            </div>
          )}

          {messages.map((message, index) => (
            <article className={`message-row ${message.role}`} key={`${message.role}-${index}-${message.content.slice(0, 12)}`}>
              {message.role === "assistant" && <div className="message-avatar">J</div>}
              <div className="message-bubble">
                {message.content}
              </div>
            </article>
          ))}

          {uploadStatus && (
            <div className="command-feedback upload-feedback">
              {uploadStatus}
            </div>
          )}

          {pendingAssistantCommand && (
            <div className="confirmation-card">
              <strong>This action is risky. Type: yes run it</strong>
              <p>{pendingAssistantCommand.message}</p>
              <div>
                <button type="button" onClick={() => void onConfirmAssistantCommand()}>yes run it</button>
                <button type="button" onClick={onCancelAssistantCommand}>Cancel</button>
              </div>
            </div>
          )}

          {!pendingAssistantCommand && lastAssistantAction?.type === "command" && (
            <div className="command-feedback">
              {lastAssistantAction.message}
            </div>
          )}

          {(isBusy || assistantActivity === "completed") && (
            <div className="thinking-line">{activityLabel(assistantActivity)}</div>
          )}
          <div ref={scrollRef} />
        </div>
      </div>

      <Composer
        disabled={isBusy}
        onSend={onSend}
        onVoiceCommand={onVoiceCommand}
        onAttachmentsSelected={onAttachmentsSelected}
        onToast={onToast}
      />
    </section>
  );
}

function activityLabel(activity: AssistantActivity) {
  switch (activity) {
    case "executing":
      return "Executing";
    case "speaking":
      return "Speaking";
    case "completed":
      return "Completed";
    case "error":
      return "Something went wrong";
    case "thinking":
    case "idle":
    default:
      return "Thinking";
  }
}