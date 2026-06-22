"use client";

import { useEffect, useState } from "react";
import { TopBar } from "./TopBar";
import { VoiceCommandHelpPanel } from "./VoiceCommandHelpPanel";
import { VoiceLoopPanel } from "./VoiceLoopPanel";
import { PanelCard } from "./ui/PanelCard";
import { StatusBadge } from "./ui/StatusBadge";
import { jarvisApi } from "@/lib/api";
import type { VoiceHealthResult, VoiceHistoryItem, VoicePipelineStatus } from "@/lib/types";

type VoicePanelProps = {
  disabled: boolean;
  onRefresh: () => Promise<void>;
  onToast: (message: string) => void;
};

export function VoicePanel({ disabled, onRefresh, onToast }: VoicePanelProps) {
  const [status, setStatus] = useState<VoicePipelineStatus | null>(null);
  const [health, setHealth] = useState<VoiceHealthResult | null>(null);
  const [history, setHistory] = useState<VoiceHistoryItem[]>([]);

  useEffect(() => {
    Promise.all([
      jarvisApi.voicePipelineStatus().then(setStatus),
      jarvisApi.voiceHealth().then(setHealth),
      jarvisApi.voiceHistory().then(setHistory)
    ]).catch(() => undefined);
  }, []);

  async function refreshVoice() {
    await onRefresh();
    setStatus(await jarvisApi.voicePipelineStatus());
    setHealth(await jarvisApi.voiceHealth());
    setHistory(await jarvisApi.voiceHistory());
  }

  return (
    <section className="tool-panel">
      <TopBar title="Voice" subtitle="Push-to-talk speech detection, Faster-Whisper, and secure command routing" />

      <div className="grid w-full max-w-5xl gap-5">
        <PanelCard className="voice-summary-card">
          <div className="voice-summary-dot" />
          <div>
            <div className="flex">
              <StatusBadge tone={isActive(status?.state) ? "green" : status?.state === "Error" ? "red" : "neutral"}>
                {status?.state ?? "Idle"}
              </StatusBadge>
            </div>
            <h3>Voice Pipeline</h3>
            <p>
              {status?.message ?? "Start listening, speak a command, then stop to transcribe and route through Jarvis Security."}
            </p>
            <div className="voice-last-grid">
              <div>
                <strong>Last transcript</strong>
                <span>{status?.lastTranscript || "None yet"}</span>
              </div>
              <div>
                <strong>Last AI response</strong>
                <span>{status?.lastAiResponse || "None yet"}</span>
              </div>
            </div>
            <div className="voice-diagnostics-grid">
              <Diagnostic label="Session" value={status?.voiceSessionId ? shortId(status.voiceSessionId) : "None"} />
              <Diagnostic label="Microphone" value={status?.microphoneStatus || health?.microphone.status || "Not checked"} />
              <Diagnostic label="Audio Size" value={formatBytes(status?.audioSizeBytes)} />
              <Diagnostic label="Recording" value={formatDuration(status?.recordingDurationMs)} />
              <Diagnostic label="Processing" value={formatDuration(status?.processingDurationMs)} />
              <Diagnostic label="STT" value={status?.sttDurationMs ? `${formatDuration(status.sttDurationMs)} ${status.sttDevice || ""}` : "Not run"} />
              <Diagnostic label="Command" value={status?.commandDetected ? `${status.commandName || "Detected"} / ${status.commandExecuted ? "executed" : "not executed"}` : "None"} />
              <Diagnostic label="Last Stage" value={status?.lastCompletedStage || "None"} />
              <Diagnostic label="Error Details" value={status?.errorDetails || "None"} />
            </div>
          </div>
        </PanelCard>

        <VoiceLoopPanel disabled={disabled} onRefresh={refreshVoice} onToast={onToast} />
        <VoiceCommandHelpPanel onToast={onToast} />

        <PanelCard className="grid gap-3">
          <h3 className="text-lg font-black text-jarvis-text">Voice Health</h3>
          <div className="voice-diagnostics-grid">
            <Diagnostic label="Microphone" value={health?.microphone.message ?? "Checked when recording starts"} />
            <Diagnostic label="Audio Capture" value={health?.audioCapture.message ?? "Unknown"} />
            <Diagnostic label="Whisper" value={health?.whisper.message ?? "Unknown"} tone={health?.whisper.available ? "ok" : "warn"} />
            <Diagnostic label="GPU / CPU" value={health?.whisper.mode ?? "GPU preferred with CPU fallback"} />
            <Diagnostic label="Ollama" value={health?.ollama.message ?? "Unknown"} tone={health?.ollama.available ? "ok" : "warn"} />
            <Diagnostic label="Voice Service" value={health?.voiceService.message ?? status?.message ?? "Idle"} />
          </div>
        </PanelCard>

        <PanelCard className="grid gap-3">
          <h3 className="text-lg font-black text-jarvis-text">Recent Voice Turns</h3>
          {history.length === 0 ? (
            <p className="text-sm text-jarvis-muted">No voice history yet.</p>
          ) : (
            <div className="grid gap-3">
              {history.slice(0, 8).map((item) => (
                <article className="rounded-2xl border border-jarvis-border bg-white/[0.035] p-4" key={item.id}>
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <strong className="text-sm text-jarvis-text">{item.transcript || "No transcript"}</strong>
                    <StatusBadge tone={item.success ? "green" : "red"}>{item.state}</StatusBadge>
                  </div>
                  <p className="mt-2 text-sm leading-6 text-jarvis-muted">{item.response || "No response recorded."}</p>
                  <div className="voice-history-meta">
                    <span>{item.commandDetected ? `Command: ${item.command || "Detected"}` : "Assistant fallback"}</span>
                    <span>{formatDuration(item.processingDurationMs)}</span>
                    <span>{item.failureReason || "No error"}</span>
                  </div>
                </article>
              ))}
            </div>
          )}
        </PanelCard>
      </div>
    </section>
  );
}

function isActive(state?: string) {
  return state === "Recording"
    || state === "Listening"
    || state === "Processing"
    || state === "Transcribing"
    || state === "Understanding"
    || state === "ExecutingCommand"
    || state === "GeneratingAIResponse";
}

function Diagnostic({ label, value, tone }: { label: string; value: string; tone?: "ok" | "warn" }) {
  return (
    <div className={tone ? `voice-diagnostic ${tone}` : "voice-diagnostic"}>
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

function shortId(value: string) {
  return value.slice(0, 8);
}
