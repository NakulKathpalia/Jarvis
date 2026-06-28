"use client";

import { ChangeEvent, FormEvent, KeyboardEvent, useRef, useState } from "react";
import { VoiceRecorderButton } from "./VoiceRecorderButton";
import {
  formatAttachmentSize,
  getAttachmentKind,
  isIngestibleAttachment,
  validateChatAttachment
} from "@/lib/chatAttachments";

type ComposerProps = {
  disabled: boolean;
  onSend: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onVoiceCommand: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onToast: (message: string) => void;
  onAttachmentsSelected?: (files: File[]) => Promise<void>;
};

export function Composer({
  disabled,
  onSend,
  onVoiceCommand,
  onToast,
  onAttachmentsSelected
}: ComposerProps) {
  const [message, setMessage] = useState("");
  const [attachmentMenuOpen, setAttachmentMenuOpen] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);

  const textAreaRef = useRef<HTMLTextAreaElement | null>(null);
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const imageInputRef = useRef<HTMLInputElement | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmed = message.trim();

    if (disabled) return;

    if (selectedFiles.length > 0 && onAttachmentsSelected) {
      const filesToUpload = [...selectedFiles];
      setSelectedFiles([]);
      await onAttachmentsSelected(filesToUpload);
    }

    if (!trimmed) {
      textAreaRef.current?.focus();
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

  function openFilePicker() {
    setAttachmentMenuOpen(false);
    fileInputRef.current?.click();
  }

  function openImagePicker() {
    setAttachmentMenuOpen(false);
    imageInputRef.current?.click();
  }

  function handleFileInputChange(event: ChangeEvent<HTMLInputElement>) {
    const files = Array.from(event.target.files ?? []);
    event.target.value = "";

    if (files.length === 0) return;

    const accepted: File[] = [];

    for (const file of files) {
      const error = validateChatAttachment(file);

      if (error) {
        onToast(`${file.name}: ${error}`);
        continue;
      }

      accepted.push(file);
    }

    if (accepted.length === 0) return;

    setSelectedFiles((current) => [...current, ...accepted]);

    const ingestibleCount = accepted.filter(isIngestibleAttachment).length;
    const attachedOnlyCount = accepted.length - ingestibleCount;

    if (attachedOnlyCount > 0) {
      onToast(`${accepted.length} file selected. ${attachedOnlyCount} will attach only until parser support is added.`);
    } else {
      onToast(`${accepted.length} file selected for Jarvis ingestion.`);
    }

    textAreaRef.current?.focus();
  }

  function removeSelectedFile(index: number) {
    setSelectedFiles((current) => current.filter((_, itemIndex) => itemIndex !== index));
  }

  const canSubmit = !disabled && (message.trim().length > 0 || selectedFiles.length > 0);

  return (
    <form className="composer-wrap" onSubmit={handleSubmit}>
      <div className="composer-box">
        <div className="attachment-root">
          <button
            className="composer-icon"
            type="button"
            title="Attach"
            onClick={() => setAttachmentMenuOpen((current) => !current)}
          >
            +
          </button>

          {attachmentMenuOpen && (
            <div className="attachment-menu">
              <button type="button" onClick={openFilePicker}>
                <span>📎</span>
                <div>
                  <strong>Upload file</strong>
                  <small>PDF, images, text, code, docs</small>
                </div>
              </button>

              <button type="button" onClick={openImagePicker}>
                <span>🖼</span>
                <div>
                  <strong>Upload image</strong>
                  <small>PNG, JPG, WEBP for OCR</small>
                </div>
              </button>

              <button type="button" onClick={() => onToast("Screenshot capture will be added later.")}>
                <span>📸</span>
                <div>
                  <strong>Take screenshot</strong>
                  <small>Coming later</small>
                </div>
              </button>
            </div>
          )}
        </div>

        <input
          ref={fileInputRef}
          type="file"
          hidden
          multiple
          accept=".pdf,.png,.jpg,.jpeg,.webp,.txt,.md,.csv,.json,.xml,.html,.css,.js,.ts,.tsx,.jsx,.cs,.py,.java,.cpp,.c,.h,.sql,.docx,.xlsx,.pptx"
          onChange={handleFileInputChange}
        />

        <input
          ref={imageInputRef}
          type="file"
          hidden
          multiple
          accept=".png,.jpg,.jpeg,.webp"
          onChange={handleFileInputChange}
        />

        <textarea
          ref={textAreaRef}
          value={message}
          rows={1}
          placeholder="Message Jarvis..."
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

        <button className="send-button" type="submit" disabled={!canSubmit} title="Send">
          ↑
        </button>
      </div>

      {selectedFiles.length > 0 && (
        <div className="attachment-preview-list">
          {selectedFiles.map((file, index) => (
            <div className="attachment-preview" key={`${file.name}-${file.size}-${index}`}>
              <span className="attachment-kind">{getAttachmentKind(file)}</span>
              <div>
                <strong>{file.name}</strong>
                <small>
                  {formatAttachmentSize(file.size)}
                  {" · "}
                  {isIngestibleAttachment(file) ? "Ready for OCR/PDF ingestion" : "Attach only"}
                </small>
              </div>
              <button type="button" onClick={() => removeSelectedFile(index)}>
                ×
              </button>
            </div>
          ))}
        </div>
      )}

      <p className="composer-help">Jarvis can make mistakes. Commands still pass through security and permissions.</p>
    </form>
  );
}