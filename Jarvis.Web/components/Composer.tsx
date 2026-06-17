"use client";

import { FormEvent, KeyboardEvent, useRef, useState } from "react";
import { VoiceRecorderButton } from "./VoiceRecorderButton";

type ComposerProps = {
  disabled: boolean;
  onSend: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onVoiceCommand: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onToast: (message: string) => void;
};

export function Composer({ disabled, onSend, onVoiceCommand, onToast }: ComposerProps) {
  const [message, setMessage] = useState("");
  const textAreaRef = useRef<HTMLTextAreaElement | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = message.trim();
    if (!trimmed || disabled) {
      return;
    }

    setMessage("");
    await onSend(trimmed);
    textAreaRef.current?.focus();
  }

  function handleKeyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      event.currentTarget.form?.requestSubmit();
    }
  }

  function handleTranscript(transcript: string) {
    setMessage((current) => (current.trim() ? `${current.trim()}\n${transcript}` : transcript));
    textAreaRef.current?.focus();
    onToast("Transcript added to composer");
  }

  async function handleVoiceAsk(transcript: string) {
    setMessage("");
    onToast("Voice sent to Jarvis");
    await onVoiceCommand(transcript);
  }

  return (
    <form className="sticky bottom-0 z-20 mx-auto w-full max-w-4xl px-4 pb-5 pt-4" onSubmit={handleSubmit}>
      <div className="rounded-[1.6rem] border border-jarvis-border bg-jarvis-panel/90 p-2 shadow-card backdrop-blur-xl focus-within:border-jarvis-green/60">
        <div className="grid grid-cols-[minmax(0,1fr)_auto_auto_auto] items-end gap-2">
          <textarea
            ref={textAreaRef}
            value={message}
            rows={1}
            placeholder="Message Jarvis"
            className="max-h-40 min-h-12 resize-y rounded-2xl bg-transparent px-4 py-3 text-sm leading-6 text-jarvis-text outline-none placeholder:text-jarvis-faint"
            onChange={(event) => setMessage(event.target.value)}
            onKeyDown={handleKeyDown}
          />
          <VoiceRecorderButton
            disabled={disabled}
            label="Mic"
            recordingLabel="Stop"
            title="Dictate"
            onToast={onToast}
            onTranscript={handleTranscript}
          />
          <VoiceRecorderButton
            disabled={disabled}
            label="Ask"
            recordingLabel="Stop"
            title="Voice ask"
            onToast={onToast}
            onTranscript={handleVoiceAsk}
          />
          <button
            className="min-h-12 rounded-2xl bg-jarvis-green px-5 text-sm font-black text-jarvis-bg transition hover:-translate-y-0.5 disabled:hover:translate-y-0"
            type="submit"
            disabled={disabled || !message.trim()}
          >
            {disabled ? "Wait" : "Send"}
          </button>
        </div>
      </div>
    </form>
  );
}
