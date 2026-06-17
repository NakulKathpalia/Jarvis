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
    <form className="composer" onSubmit={handleSubmit}>
      <div className="composer-input-wrap">
        <textarea
          ref={textAreaRef}
          value={message}
          rows={1}
          placeholder="Message Jarvis"
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
      </div>
      <button type="submit" disabled={disabled || !message.trim()}>
        {disabled ? "Wait" : "Send"}
      </button>
    </form>
  );
}
