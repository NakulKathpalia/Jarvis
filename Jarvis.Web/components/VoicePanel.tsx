"use client";

import { useEffect, useRef, useState } from "react";
import { TopBar } from "./TopBar";
import { VoiceCommandHelpPanel } from "./VoiceCommandHelpPanel";
import { VoiceLoopPanel } from "./VoiceLoopPanel";
import { PanelCard } from "./ui/PanelCard";
import { StatusBadge } from "./ui/StatusBadge";
import { jarvisApi } from "@/lib/api";
import type { JarvisStatus, VoiceHealthResult, VoiceHistoryItem, VoicePipelineStatus } from "@/lib/types";

type VoicePanelProps = {
  disabled: boolean;
  appStatus: JarvisStatus | null;
  memoryCount: number;
  onRefresh: () => Promise<void>;
  onToast: (message: string) => void;
};

export function VoicePanel({ disabled, appStatus, memoryCount, onRefresh, onToast }: VoicePanelProps) {
  const [status, setStatus] = useState<VoicePipelineStatus | null>(null);
  const [health, setHealth] = useState<VoiceHealthResult | null>(null);
  const [history, setHistory] = useState<VoiceHistoryItem[]>([]);
  const [playbackStatus, setPlaybackStatus] = useState("Idle");
  const currentSpeechRef = useRef<HTMLAudioElement | null>(null);

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

  async function playAudioUrl(audioUrl: string) {
    stopSpeaking();
    const audio = new Audio(resolveAudioUrl(audioUrl));
    currentSpeechRef.current = audio;
    setPlaybackStatus("Playback starting");
    audio.addEventListener("play", () => setPlaybackStatus("Playback started"));
    audio.addEventListener("ended", () => {
      if (currentSpeechRef.current === audio) {
        currentSpeechRef.current = null;
      }
      setPlaybackStatus("Playback completed");
    });
    audio.addEventListener("error", () => {
      setPlaybackStatus("Playback failed: browser could not load generated audio.");
    });

    try {
      await audio.play();
    } catch (error) {
      const message = error instanceof Error
        ? `Playback failed: ${error.message}`
        : "Playback failed. Browser audio permission may be blocking playback.";
      setPlaybackStatus(message);
      onToast(message);
      throw error;
    }
  }

  async function speakLastResponse() {
    const text = status?.spokenResponse || status?.lastAiResponse || history[0]?.spokenResponse || history[0]?.response;
    if (!text?.trim()) {
      onToast("No response to speak yet.");
      return;
    }

    const result = await jarvisApi.speak(text);
    if (!result.audioUrl) {
      onToast(result.message || "No audio returned");
      return;
    }

    await playAudioUrl(result.audioUrl);
    onToast("Speaking last response");
  }

  async function testVoice() {
    const result = await jarvisApi.speak("Ji sir, Jarvis voice system ready hai.");
    if (!result.audioUrl) {
      onToast(result.message || "No audio returned");
      return;
    }

    await playAudioUrl(result.audioUrl);
    onToast("Test voice playing");
  }

  function stopSpeaking() {
    const audio = currentSpeechRef.current;
    if (audio) {
      audio.pause();
      audio.currentTime = 0;
      currentSpeechRef.current = null;
    }
    setPlaybackStatus("Playback stopped");
    void jarvisApi.stopSpeaking().catch(() => undefined);
  }

  return (
    <section className="tool-panel">
      <TopBar title="Voice" subtitle="Jarvis online. Push-to-talk is ready when you are." />

      <div className="grid w-full max-w-5xl gap-5">
        <PanelCard className="voice-summary-card">
          <div className="voice-summary-dot" />
          <div>
            <div className="flex">
              <StatusBadge tone={isActive(status?.state) ? "green" : status?.state === "Error" ? "red" : "neutral"}>
                {status?.state ?? "Idle"}
              </StatusBadge>
            </div>
            <h3>Jarvis online.</h3>
            <p>
              {status?.message ?? "Start Listening, speak naturally, then Stop Listening."}
            </p>
            <div className="voice-diagnostics-grid">
              <Diagnostic label="AI Status" value={health?.ollama.available || appStatus?.online ? "Ready" : "Check"} tone={health?.ollama.available || appStatus?.online ? "ok" : "warn"} />
              <Diagnostic label="Voice Status" value={friendlyVoiceState(status?.state ?? health?.voiceService.status ?? "Idle")} tone={status?.state === "Error" ? "warn" : "ok"} />
              <Diagnostic label="Memory Status" value={`${memoryCount} memories`} />
              <Diagnostic label="Model Status" value={appStatus?.model ?? "Unknown"} tone={appStatus?.online ? "ok" : "warn"} />
            </div>
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
              <Diagnostic label="TTS" value={status?.ttsProvider ? `${status.ttsProvider} / ${status.voiceUsed || "default"}` : "Not run"} />
              <Diagnostic label="Speech" value={status?.speechDurationMs ? `${formatDuration(status.speechDurationMs)} ${status.playbackReady ? "ready" : ""}` : "0 ms"} />
              <Diagnostic label="Command" value={status?.commandDetected ? `${status.commandName || "Detected"} / ${status.commandExecuted ? "executed" : "not executed"}` : "None"} />
              <Diagnostic label="Last Stage" value={status?.lastCompletedStage || "None"} />
              <Diagnostic label="Error Details" value={status?.errorDetails || "None"} />
            </div>
          </div>
        </PanelCard>

        <VoiceLoopPanel
          disabled={disabled}
          onRefresh={refreshVoice}
          onToast={onToast}
          onPlayAudio={playAudioUrl}
          onStopSpeaking={stopSpeaking}
        />
        <VoiceCommandHelpPanel onToast={onToast} />

        <PanelCard className="grid gap-3">
          <h3 className="text-lg font-black text-jarvis-text">Voice Responses</h3>
          <div className="voice-diagnostics-grid">
            <Diagnostic label="TTS Enabled" value={health?.tts.available ? "Available" : health?.tts.message ?? "Unknown"} tone={health?.tts.available ? "ok" : "warn"} />
            <Diagnostic label="Current Voice" value={health?.tts.voiceName ?? "Unknown"} />
            <Diagnostic label="Provider" value={health?.tts.provider ?? "Unknown"} />
            <Diagnostic label="Playback" value={health?.tts.playbackCapability ?? "Unknown"} />
            <Diagnostic label="Current Status" value={playbackStatus} />
          </div>
          <div className="voice-loop-actions">
            <button className="soft-button" type="button" onClick={() => void speakLastResponse()}>
              Speak Last Response
            </button>
            <button className="soft-button" type="button" onClick={() => void testVoice()}>
              Test Voice
            </button>
            <button className="danger-button" type="button" onClick={stopSpeaking}>
              Stop Speaking
            </button>
          </div>
        </PanelCard>

        <PanelCard className="grid gap-3">
          <h3 className="text-lg font-black text-jarvis-text">Voice Health</h3>
          <div className="voice-diagnostics-grid">
            <Diagnostic label="Microphone" value={health?.microphone.message ?? "Checked when recording starts"} />
            <Diagnostic label="Audio Capture" value={health?.audioCapture.message ?? "Unknown"} />
            <Diagnostic label="Whisper" value={health?.whisper.message ?? "Unknown"} tone={health?.whisper.available ? "ok" : "warn"} />
            <Diagnostic label="GPU / CPU" value={health?.whisper.mode ?? "GPU preferred with CPU fallback"} />
            <Diagnostic label="TTS" value={health?.tts.message ?? "Unknown"} tone={health?.tts.available ? "ok" : "warn"} />
            <Diagnostic label="OCR" value={health?.ocr.message ?? "Unknown"} tone={health?.ocr.available ? "ok" : "warn"} />
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
                    <span>{item.ttsProvider || "No TTS"}</span>
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
    || state === "GeneratingAIResponse"
    || state === "Speaking";
}

function friendlyVoiceState(state: string) {
  return state === "GeneratingAIResponse" ? "Thinking"
    : state === "ExecutingCommand" ? "Executing"
      : state === "CommandDetected" ? "Command detected"
        : state;
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

function resolveAudioUrl(audioUrl: string) {
  if (audioUrl.startsWith("http://") || audioUrl.startsWith("https://")) {
    return audioUrl;
  }

  return `${jarvisApi.apiBaseUrl}${audioUrl.startsWith("/") ? audioUrl : `/${audioUrl}`}`;
}
