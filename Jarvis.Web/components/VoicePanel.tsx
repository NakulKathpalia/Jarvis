"use client";

import { useEffect, useState } from "react";
import { TopBar } from "./TopBar";
import { VoiceCommandHelpPanel } from "./VoiceCommandHelpPanel";
import { VoiceLoopPanel } from "./VoiceLoopPanel";
import { WakeWordPanel } from "./WakeWordPanel";
import { AssistantOrb } from "./ui/AssistantOrb";
import { PanelCard } from "./ui/PanelCard";
import { StatusBadge } from "./ui/StatusBadge";
import { jarvisApi } from "@/lib/api";
import type { VoiceHistoryItem, VoicePipelineStatus } from "@/lib/types";

type VoicePanelProps = {
  disabled: boolean;
  onRefresh: () => Promise<void>;
  onToast: (message: string) => void;
};

export function VoicePanel({ disabled, onRefresh, onToast }: VoicePanelProps) {
  const [wakeSignal, setWakeSignal] = useState(0);
  const [status, setStatus] = useState<VoicePipelineStatus | null>(null);
  const [history, setHistory] = useState<VoiceHistoryItem[]>([]);

  useEffect(() => {
    Promise.all([
      jarvisApi.voicePipelineStatus().then(setStatus),
      jarvisApi.voiceHistory().then(setHistory)
    ]).catch(() => undefined);
  }, []);

  async function refreshVoice() {
    await onRefresh();
    setStatus(await jarvisApi.voicePipelineStatus());
    setHistory(await jarvisApi.voiceHistory());
  }

  return (
    <section className="tool-panel">
      <TopBar title="Voice" subtitle="Microphone to Whisper, command routing, Ollama, and Piper" />

      <div className="grid w-full max-w-5xl gap-5">
        <PanelCard className="grid place-items-center gap-5 text-center">
          <AssistantOrb size="lg" active={isActive(status?.state)} label="Mic" />
          <div>
            <div className="flex justify-center">
              <StatusBadge tone={isActive(status?.state) ? "green" : status?.state === "Error" ? "red" : "neutral"}>
                {status?.state ?? "Idle"}
              </StatusBadge>
            </div>
            <h3 className="mt-4 text-2xl font-black text-jarvis-text">Voice Pipeline</h3>
            <p className="mt-2 max-w-2xl text-sm leading-7 text-jarvis-muted">
              {status?.message ?? "Start a voice turn to record, transcribe, detect commands, and optionally speak back."}
            </p>
          </div>
        </PanelCard>

        <VoiceLoopPanel disabled={disabled} onRefresh={refreshVoice} onToast={onToast} wakeSignal={wakeSignal} />
        <WakeWordPanel onToast={onToast} onWakeActivated={() => setWakeSignal((current) => current + 1)} />
        <VoiceCommandHelpPanel onToast={onToast} />

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
    || state === "Transcribing"
    || state === "WakeWordChecking"
    || state === "ExecutingCommand"
    || state === "GeneratingAIResponse"
    || state === "Speaking";
}
