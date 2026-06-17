"use client";

import { useRef, useState } from "react";
import { jarvisApi } from "@/lib/api";
import { startVoiceCapture, type VoiceCaptureSession } from "@/lib/voiceCapture";

type VoiceRecorderButtonProps = {
  disabled: boolean;
  label: string;
  recordingLabel: string;
  title: string;
  onTranscript: (text: string) => void | Promise<void>;
  onToast: (message: string) => void;
};

type RecorderState = "idle" | "recording" | "processing";

export function VoiceRecorderButton({
  disabled,
  label,
  recordingLabel,
  title,
  onTranscript,
  onToast
}: VoiceRecorderButtonProps) {
  const [state, setState] = useState<RecorderState>("idle");
  const captureRef = useRef<VoiceCaptureSession | null>(null);

  async function toggleRecording() {
    if (disabled || state === "processing") {
      return;
    }

    if (state === "recording") {
      await stopRecording();
      return;
    }

    await startRecording();
  }

  async function startRecording() {
    try {
      captureRef.current = await startVoiceCapture();
      setState("recording");
      onToast(`${title} recording started`);
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Microphone permission failed");
      setState("idle");
    }
  }

  async function stopRecording() {
    const capture = captureRef.current;
    if (!capture) {
      setState("idle");
      return;
    }

    setState("processing");
    captureRef.current = null;

    try {
      const wav = await capture.stop();
      const result = await jarvisApi.transcribeVoice(wav);

      if (result.transcript.trim()) {
        await onTranscript(result.transcript.trim());
      } else {
        onToast(result.message || "No transcript returned");
      }
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Voice transcription failed");
    } finally {
      setState("idle");
    }
  }

  const ariaLabel =
    state === "recording" ? `Stop ${title}` : state === "processing" ? `Processing ${title}` : title;

  return (
    <button
      aria-label={ariaLabel}
      className={`voice-button ${state}`}
      disabled={disabled || state === "processing"}
      title={ariaLabel}
      type="button"
      onClick={toggleRecording}
    >
      {state === "recording" ? recordingLabel : state === "processing" ? "..." : label}
    </button>
  );
}
