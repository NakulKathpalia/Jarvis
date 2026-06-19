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
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "VoiceRecording",
        stage: "mic-clicked",
        status: "Started",
        message: `${title} microphone recording requested.`
      }).catch(() => undefined);
      captureRef.current = await startVoiceCapture();
      setState("recording");
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "VoiceRecording",
        stage: "recording-started",
        status: "Success",
        message: `${title} recording started.`
      }).catch(() => undefined);
      onToast(`${title} recording started`);
    } catch (error) {
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "Error",
        stage: "recording-start-failed",
        status: "Failed",
        message: "Microphone recording failed to start.",
        error: error instanceof Error ? error.message : "Microphone permission failed"
      }).catch(() => undefined);
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
    void jarvisApi.logInteraction({
      source: "Voice",
      type: "VoiceRecording",
      stage: "recording-stopped",
      status: "Success",
      message: `${title} recording stopped; uploading audio.`
    }).catch(() => undefined);

    try {
      const wav = await capture.stop();
      const result = await jarvisApi.transcribeVoice(wav);

      if (result.transcript.trim()) {
        await onTranscript(result.transcript.trim());
      } else {
        onToast(result.message || "No transcript returned");
      }
    } catch (error) {
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "Error",
        stage: "transcription-request-failed",
        status: "Failed",
        message: "Voice transcription request failed.",
        error: error instanceof Error ? error.message : "Voice transcription failed"
      }).catch(() => undefined);
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
