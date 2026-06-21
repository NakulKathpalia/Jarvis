"use client";

import { useEffect, useRef, useState } from "react";
import { jarvisApi } from "@/lib/api";
import type { VoicePipelineResult, VoicePipelineState, VoicePipelineStatus } from "@/lib/types";
import { startVoiceCapture, type VoiceCaptureSession } from "@/lib/voiceCapture";

type VoiceLoopPanelProps = {
  disabled: boolean;
  onRefresh: () => Promise<void>;
  onToast: (message: string) => void;
};

const activeStates: VoicePipelineState[] = [
  "Recording",
  "Transcribing",
  "CommandDetected",
  "ExecutingCommand",
  "GeneratingAIResponse"
];

export function VoiceLoopPanel({ disabled, onRefresh, onToast }: VoiceLoopPanelProps) {
  const [isActive, setIsActive] = useState(false);
  const [status, setStatus] = useState<VoicePipelineStatus | null>(null);
  const [pending, setPending] = useState<VoicePipelineResult | null>(null);
  const captureRef = useRef<VoiceCaptureSession | null>(null);

  useEffect(() => {
    jarvisApi.voicePipelineStatus().then(setStatus).catch(() => undefined);
  }, []);

  async function activateLoop() {
    if (disabled || isBusy(status?.state)) {
      onToast("Jarvis is busy.");
      return;
    }

    try {
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "VoiceRecording",
        stage: "push-to-talk-start",
        status: "Started",
        message: "Push-to-talk voice recording started."
      }).catch(() => undefined);
      captureRef.current = await startVoiceCapture();
      setIsActive(true);
      setStatus({
        state: "Recording",
        updatedAtUtc: new Date().toISOString(),
        lastTranscript: status?.lastTranscript ?? "",
        lastAiResponse: status?.lastAiResponse ?? "",
        message: "Listening. Stop when you finish speaking."
      });
      onToast("Listening");
    } catch (error) {
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "Error",
        stage: "pipeline-recording-start-failed",
        status: "Failed",
        message: "Voice pipeline recording failed to start.",
        error: error instanceof Error ? error.message : "Microphone permission failed"
      }).catch(() => undefined);
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
    void jarvisApi.logInteraction({
      source: "Voice",
      type: "VoiceRecording",
      stage: "pipeline-recording-stopped",
      status: "Success",
      message: "Voice pipeline recording stopped; uploading audio."
    }).catch(() => undefined);

    try {
      const wav = await capture.stop();
      const result = await jarvisApi.runVoicePipeline(wav);
      await handlePipelineResult(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Voice pipeline failed";
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "Error",
        stage: "pipeline-request-failed",
        status: "Failed",
        message: "Voice pipeline request failed.",
        error: message
      }).catch(() => undefined);
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
          <p>{status?.message ?? "Start a push-to-talk voice turn."}</p>
          <span>{currentState} - {formatTimestamp(status?.updatedAtUtc)}</span>
        </div>
      </div>

      <div className="voice-loop-transcript" title={transcript || aiResponse}>
        {transcript || aiResponse || "No voice turn yet."}
      </div>

      <div className="voice-loop-actions">
        {!isActive && !pending && (
          <button className="soft-button" type="button" disabled={disabled} onClick={activateLoop}>
            Start Listening
          </button>
        )}
        {isActive && (
          <button className="danger-button" type="button" onClick={finishTurn}>
            Stop Listening
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
