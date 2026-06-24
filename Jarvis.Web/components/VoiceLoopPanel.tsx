"use client";

import { useEffect, useRef, useState } from "react";
import { jarvisApi } from "@/lib/api";
import type { VoicePipelineResult, VoicePipelineState, VoicePipelineStatus } from "@/lib/types";
import { startVoiceCapture, type VoiceCaptureSession } from "@/lib/voiceCapture";

type VoiceLoopPanelProps = {
  disabled: boolean;
  onRefresh: () => Promise<void>;
  onToast: (message: string) => void;
  onPlayAudio: (audioUrl: string) => Promise<void>;
  onStopSpeaking: () => void;
};

const activeStates: VoicePipelineState[] = [
  "Listening",
  "Recording",
  "Processing",
  "Transcribing",
  "Understanding",
  "CommandDetected",
  "ExecutingCommand",
  "GeneratingAIResponse",
  "Speaking"
];

export function VoiceLoopPanel({ disabled, onRefresh, onToast, onPlayAudio, onStopSpeaking }: VoiceLoopPanelProps) {
  const [isActive, setIsActive] = useState(false);
  const [status, setStatus] = useState<VoicePipelineStatus | null>(null);
  const [pending, setPending] = useState<VoicePipelineResult | null>(null);
  const captureRef = useRef<VoiceCaptureSession | null>(null);
  const recordingStartedAtRef = useRef<number | null>(null);

  useEffect(() => {
    jarvisApi.voicePipelineStatus().then(setStatus).catch(() => undefined);
  }, []);

  async function activateLoop() {
    if (disabled || isBusy(status?.state)) {
      onToast("Jarvis is busy.");
      return;
    }

    try {
      onStopSpeaking();
      void jarvisApi.logInteraction({
        source: "Voice",
        type: "VoiceRecording",
        stage: "push-to-talk-start",
        status: "Started",
        message: "Push-to-talk voice recording started."
      }).catch(() => undefined);
      captureRef.current = await startVoiceCapture();
      recordingStartedAtRef.current = Date.now();
      setIsActive(true);
      setStatus({
        ...emptyStatus(status),
        state: "Recording",
        updatedAtUtc: new Date().toISOString(),
        startedAtUtc: new Date().toISOString(),
        lastTranscript: status?.lastTranscript ?? "",
        lastAiResponse: status?.lastAiResponse ?? "",
        microphoneStatus: "Granted",
        message: "Listening..."
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
    const recordingDurationMs = recordingStartedAtRef.current ? Date.now() - recordingStartedAtRef.current : 0;
    setStatus({
      ...toStatus("Processing", "Processing audio...", status),
      recordingDurationMs
    });
    void jarvisApi.logInteraction({
      source: "Voice",
      type: "VoiceRecording",
      stage: "pipeline-recording-stopped",
      status: "Success",
      message: "Voice pipeline recording stopped; uploading audio."
    }).catch(() => undefined);

    try {
      const wav = await capture.stop();
      setStatus({
        ...toStatus("Transcribing", "Transcribing...", status),
        recordingDurationMs,
        audioSizeBytes: wav.size
      });
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
      recordingStartedAtRef.current = null;
    }
  }

  async function confirmPending() {
    const confirmationId = pending?.confirmationId;
    if (!confirmationId) {
      return;
    }

    setStatus(toStatus("ExecutingCommand", "Executing...", status));
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
    setStatus(toStatus("Completed", "Cancelled.", status));
  }

  function stopLoop() {
    captureRef.current?.cancel();
    captureRef.current = null;
    setIsActive(false);
    recordingStartedAtRef.current = null;
    setPending(null);
    setStatus(toStatus("Idle", "Stopped.", status));
    onToast("Stopped");
  }

  async function handlePipelineResult(result: VoicePipelineResult) {
    setStatus({
      ...emptyStatus(status),
      state: result.state,
      updatedAtUtc: new Date().toISOString(),
      lastTranscript: result.transcript || status?.lastTranscript || "",
      lastAiResponse: result.aiResponse || status?.lastAiResponse || "",
      message: result.message,
      voiceSessionId: result.voiceSessionId,
      startedAtUtc: result.startedAtUtc,
      endedAtUtc: result.endedAtUtc,
      audioSizeBytes: result.audioSizeBytes,
      recordingDurationMs: result.recordingDurationMs,
      processingDurationMs: result.processingDurationMs,
      sttDurationMs: result.sttDurationMs,
      commandDurationMs: result.commandDurationMs,
      speechDurationMs: result.speechDurationMs,
      commandDetected: result.commandDetected,
      commandExecuted: result.commandExecuted,
      commandName: result.commandName,
      errorDetails: result.failureReason,
      lastCompletedStage: result.lastCompletedStage,
      sttDevice: result.sttDevice,
      spokenResponse: result.spokenResponse,
      ttsProvider: result.ttsProvider,
      voiceUsed: result.voiceUsed,
      playbackReady: result.playbackReady,
      playbackFailureReason: result.playbackFailureReason
    });

    if (result.requiresConfirmation) {
      setPending(result);
      onToast("Voice command needs confirmation");
      return;
    }

    if (result.audioUrl) {
      await onPlayAudio(result.audioUrl);
    }

    await onRefresh();
  }

  const currentState = status?.state ?? "Idle";
  const transcript = status?.lastTranscript ?? "";
  const aiResponse = status?.lastAiResponse ?? "";
  const details = status ?? emptyStatus(null);

  return (
    <section className={isActive || isBusy(currentState) ? "voice-loop active" : "voice-loop"}>
      <div className="voice-loop-main">
        <span className="voice-loop-dot" />
        <div>
          <strong>Push-to-talk</strong>
          <p>{status?.message ?? "Start a push-to-talk voice turn."}</p>
          <span>{currentState} - {formatTimestamp(status?.updatedAtUtc)}</span>
        </div>
      </div>

      <div className="voice-loop-transcript" title={transcript || aiResponse}>
        {transcript || aiResponse || "No voice turn yet."}
      </div>

      <div className="voice-diagnostics-grid compact">
        <Diagnostic label="Microphone" value={details.microphoneStatus || (isActive ? "Granted" : "Not checked")} />
        <Diagnostic label="Recording" value={formatDuration(details.recordingDurationMs)} />
        <Diagnostic label="Audio Size" value={formatBytes(details.audioSizeBytes)} />
        <Diagnostic label="Last Stage" value={details.lastCompletedStage || "None"} />
        <Diagnostic label="STT" value={details.sttDurationMs ? `${formatDuration(details.sttDurationMs)} ${details.sttDevice || ""}` : "Not run"} />
        <Diagnostic label="TTS" value={details.ttsProvider ? `${details.ttsProvider} / ${details.voiceUsed || "default"}` : "Not run"} />
        <Diagnostic label="Speech" value={details.speechDurationMs ? `${formatDuration(details.speechDurationMs)} ${details.playbackReady ? "ready" : ""}` : "0 ms"} />
        <Diagnostic label="Command" value={details.commandDetected ? `${details.commandName || "Detected"} / ${details.commandExecuted ? "executed" : "not executed"}` : "None"} />
        <Diagnostic label="Error" value={details.errorDetails || "None"} />
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
    ...emptyStatus(previous),
    state,
    message,
    updatedAtUtc: new Date().toISOString(),
    lastTranscript: previous?.lastTranscript ?? "",
    lastAiResponse: previous?.lastAiResponse ?? ""
  };
}

function emptyStatus(previous?: VoicePipelineStatus | null): VoicePipelineStatus {
  return {
    state: previous?.state ?? "Idle",
    message: previous?.message ?? "",
    updatedAtUtc: previous?.updatedAtUtc ?? new Date().toISOString(),
    lastTranscript: previous?.lastTranscript ?? "",
    lastAiResponse: previous?.lastAiResponse ?? "",
    voiceSessionId: previous?.voiceSessionId ?? "",
    startedAtUtc: previous?.startedAtUtc ?? null,
    endedAtUtc: previous?.endedAtUtc ?? null,
    audioSizeBytes: previous?.audioSizeBytes ?? 0,
    recordingDurationMs: previous?.recordingDurationMs ?? 0,
    processingDurationMs: previous?.processingDurationMs ?? 0,
    sttDurationMs: previous?.sttDurationMs ?? 0,
    commandDurationMs: previous?.commandDurationMs ?? 0,
    speechDurationMs: previous?.speechDurationMs ?? 0,
    commandDetected: previous?.commandDetected ?? false,
    commandExecuted: previous?.commandExecuted ?? false,
    commandName: previous?.commandName ?? "",
    errorDetails: previous?.errorDetails ?? "",
    lastCompletedStage: previous?.lastCompletedStage ?? "",
    microphoneStatus: previous?.microphoneStatus ?? "Not checked",
    sttDevice: previous?.sttDevice ?? "",
    spokenResponse: previous?.spokenResponse ?? "",
    ttsProvider: previous?.ttsProvider ?? "",
    voiceUsed: previous?.voiceUsed ?? "",
    playbackReady: previous?.playbackReady ?? false,
    playbackFailureReason: previous?.playbackFailureReason ?? ""
  };
}

function Diagnostic({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <strong>{label}</strong>
      <span>{value}</span>
    </div>
  );
}

function formatDuration(value?: number) {
  if (!value) {
    return "0 ms";
  }

  return value >= 1000 ? `${(value / 1000).toFixed(1)} s` : `${value} ms`;
}

function formatBytes(value?: number) {
  if (!value) {
    return "0 B";
  }

  return value >= 1024 * 1024
    ? `${(value / (1024 * 1024)).toFixed(2)} MB`
    : `${Math.round(value / 1024)} KB`;
}

function formatTimestamp(value?: string) {
  if (!value) {
    return "not run";
  }

  return new Date(value).toLocaleTimeString();
}
