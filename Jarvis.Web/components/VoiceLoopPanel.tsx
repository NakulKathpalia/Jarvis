"use client";

import { useEffect, useRef, useState } from "react";
import { jarvisApi } from "@/lib/api";
import type { VoicePipelineResult, VoicePipelineState, VoicePipelineStatus } from "@/lib/types";
import { startVoiceCapture, type VoiceCaptureSession } from "@/lib/voiceCapture";

type VoiceLoopPanelProps = {
  disabled: boolean;
  onRefresh: () => Promise<void>;
  onToast: (message: string) => void;
  wakeSignal: number;
};

type VoiceLoopSource = "manual" | "wake";

const activeStates: VoicePipelineState[] = [
  "Recording",
  "Transcribing",
  "WakeWordChecking",
  "CommandDetected",
  "ExecutingCommand",
  "GeneratingAIResponse",
  "Speaking"
];

export function VoiceLoopPanel({ disabled, onRefresh, onToast, wakeSignal }: VoiceLoopPanelProps) {
  const [isActive, setIsActive] = useState(false);
  const [status, setStatus] = useState<VoicePipelineStatus | null>(null);
  const [pending, setPending] = useState<VoicePipelineResult | null>(null);
  const captureRef = useRef<VoiceCaptureSession | null>(null);

  useEffect(() => {
    jarvisApi.voicePipelineStatus().then(setStatus).catch(() => undefined);
  }, []);

  useEffect(() => {
    if (wakeSignal <= 0) {
      return;
    }

    void activateLoop("wake");
  }, [wakeSignal]);

  async function activateLoop(source: VoiceLoopSource) {
    if (disabled || isBusy(status?.state)) {
      onToast("Jarvis is busy.");
      return;
    }

    try {
      captureRef.current = await startVoiceCapture();
      setIsActive(true);
      setStatus({
        state: "Recording",
        updatedAtUtc: new Date().toISOString(),
        lastTranscript: status?.lastTranscript ?? "",
        lastAiResponse: status?.lastAiResponse ?? "",
        message: source === "wake" ? "Wake word activated. Recording." : "Recording voice turn."
      });
      onToast(source === "wake" ? "Wake word activated. Recording." : "Recording voice turn");
    } catch (error) {
      setIsActive(false);
      setStatus(toStatus("Error", error instanceof Error ? error.message : "Microphone permission failed"));
      onToast(error instanceof Error ? error.message : "Microphone permission failed");
    }
  }

  async function finishTurn() {
    const capture = captureRef.current;
    if (!capture) {
      return;
    }

    captureRef.current = null;
    setStatus(toStatus("Transcribing", "Sending audio to the local voice pipeline.", status));

    try {
      const wav = await capture.stop();
      const result = await jarvisApi.runVoicePipeline(wav);
      await handlePipelineResult(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Voice pipeline failed";
      setStatus(toStatus("Error", message, status));
      onToast(message);
    } finally {
      setIsActive(false);
    }
  }

  async function confirmPending() {
    const confirmationId = pending?.confirmationId;
    if (!confirmationId) {
      return;
    }

    setStatus(toStatus("ExecutingCommand", "Executing confirmed voice command.", status));
    try {
      const result = await jarvisApi.confirmVoicePipeline(confirmationId);
      setPending(null);
      await handlePipelineResult(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Voice confirmation failed";
      setStatus(toStatus("Error", message, status));
      onToast(message);
    }
  }

  function cancelPending() {
    setPending(null);
    setStatus(toStatus("Completed", "Voice command cancelled locally.", status));
  }

  function stopLoop() {
    captureRef.current?.cancel();
    captureRef.current = null;
    setIsActive(false);
    setPending(null);
    setStatus(toStatus("Idle", "Voice loop stopped.", status));
    onToast("Voice loop stopped");
  }

  async function handlePipelineResult(result: VoicePipelineResult) {
    setStatus({
      state: result.state,
      updatedAtUtc: new Date().toISOString(),
      lastTranscript: result.transcript || status?.lastTranscript || "",
      lastAiResponse: result.aiResponse || status?.lastAiResponse || "",
      message: result.message
    });

    if (result.requiresConfirmation) {
      setPending(result);
      onToast("Voice command needs confirmation");
      return;
    }

    if (result.audioUrl) {
      setStatus(toStatus("Speaking", "Playing Piper audio.", status));
      await new Audio(result.audioUrl).play();
      setStatus(toStatus("Completed", result.message, status));
    }

    await onRefresh();
  }

  const currentState = status?.state ?? "Idle";
  const transcript = status?.lastTranscript ?? "";
  const aiResponse = status?.lastAiResponse ?? "";

  return (
    <section className={isActive || isBusy(currentState) ? "voice-loop active" : "voice-loop"}>
      <div className="voice-loop-main">
        <span className="voice-loop-dot" />
        <div>
          <strong>Voice Pipeline</strong>
          <p>{status?.message ?? "Start a hands-free turn."}</p>
          <span>{currentState} - {formatTimestamp(status?.updatedAtUtc)}</span>
        </div>
      </div>

      <div className="voice-loop-transcript" title={transcript || aiResponse}>
        {transcript || aiResponse || "No voice turn yet."}
      </div>

      <div className="voice-loop-actions">
        {!isActive && !pending && (
          <button className="soft-button" type="button" disabled={disabled} onClick={() => activateLoop("manual")}>
            Start
          </button>
        )}
        {isActive && (
          <button className="danger-button" type="button" onClick={finishTurn}>
            Finish
          </button>
        )}
        {pending && (
          <button className="primary-button" type="button" onClick={confirmPending}>
            Confirm
          </button>
        )}
        {pending && (
          <button className="soft-button" type="button" onClick={cancelPending}>
            Cancel
          </button>
        )}
        {(isActive || pending) && (
          <button className="soft-button" type="button" onClick={stopLoop}>
            Stop
          </button>
        )}
      </div>
    </section>
  );
}

function isBusy(state?: VoicePipelineState) {
  return state ? activeStates.includes(state) : false;
}

function toStatus(
  state: VoicePipelineState,
  message: string,
  previous?: VoicePipelineStatus | null
): VoicePipelineStatus {
  return {
    state,
    message,
    updatedAtUtc: new Date().toISOString(),
    lastTranscript: previous?.lastTranscript ?? "",
    lastAiResponse: previous?.lastAiResponse ?? ""
  };
}

function formatTimestamp(value?: string) {
  if (!value) {
    return "not run";
  }

  return new Date(value).toLocaleTimeString();
}
