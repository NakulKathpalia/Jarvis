"use client";

import { useEffect, useRef, useState } from "react";
import { jarvisApi } from "@/lib/api";
import { startVoiceCapture, type VoiceCaptureSession } from "@/lib/voiceCapture";
import type { VoiceStatus } from "@/lib/types";

type WakeWordPanelProps = {
  onToast: (message: string) => void;
  onWakeActivated: () => void;
};

export function WakeWordPanel({ onToast, onWakeActivated }: WakeWordPanelProps) {
  const [status, setStatus] = useState<VoiceStatus["wakeWord"] | null>(null);
  const [isArmed, setIsArmed] = useState(false);
  const [state, setState] = useState<"idle" | "listening" | "checking">("idle");
  const [lastTranscript, setLastTranscript] = useState("");
  const captureRef = useRef<VoiceCaptureSession | null>(null);
  const listenTimerRef = useRef<number | null>(null);

  useEffect(() => {
    jarvisApi
      .wakeStatus()
      .then(setStatus)
      .catch((error: Error) => onToast(error.message));
  }, [onToast]);

  useEffect(() => {
    return () => {
      clearListenTimer();
      captureRef.current?.cancel();
    };
  }, []);

  async function toggleArmed() {
    if (!status?.enabled) {
      onToast("Wake word is disabled in Settings.");
      return;
    }

    if (!status.configured) {
      onToast(status.message);
      return;
    }

    if (isArmed) {
      clearListenTimer();
      captureRef.current?.cancel();
      captureRef.current = null;
      setIsArmed(false);
      setState("idle");
      onToast("Wake word disarmed");
      return;
    }

    setIsArmed(true);
    onToast("Wake word armed");
    await listenForWakeWord();
  }

  async function listenForWakeWord() {
    clearListenTimer();

    try {
      captureRef.current = await startVoiceCapture();
      setState("listening");
      setLastTranscript("");
      onToast(`Listening for "${status?.wakeWordPhrase || "jarvis"}"`);
      listenTimerRef.current = window.setTimeout(() => void finishWakeCheck(), 3200);
    } catch (error) {
      setIsArmed(false);
      setState("idle");
      onToast(error instanceof Error ? error.message : "Microphone permission failed");
    }
  }

  async function finishWakeCheck() {
    const capture = captureRef.current;
    if (!capture) {
      setState(isArmed ? "idle" : "idle");
      return;
    }

    clearListenTimer();
    captureRef.current = null;
    setState("checking");

    try {
      const wav = await capture.stop();
      const transcription = await jarvisApi.transcribeVoice(wav);
      const transcript = transcription.transcript.trim();
      setLastTranscript(transcript);

      if (!transcript) {
        onToast(transcription.message || "No wake transcript returned");
        setState("idle");
        return;
      }

      const result = await jarvisApi.wakeCheck(transcript);
      onToast(result.message);

      if (result.detected) {
        setState("idle");
        onWakeActivated();
        return;
      }

      setState("idle");
    } catch (error) {
      setState("idle");
      onToast(error instanceof Error ? error.message : "Wake check failed");
    }
  }

  function clearListenTimer() {
    if (listenTimerRef.current !== null) {
      window.clearTimeout(listenTimerRef.current);
      listenTimerRef.current = null;
    }
  }

  const statusText = status
    ? state === "listening"
      ? `Listening for "${status.wakeWordPhrase}".`
      : state === "checking"
      ? "Checking wake phrase locally."
      : isArmed
      ? "Wake word armed. Voice loop handoff ready."
      : status.enabled
      ? status.message
      : "Wake word disabled"
    : "Checking wake word";

  return (
    <section className={isArmed ? "wake-word-panel armed" : "wake-word-panel"}>
      <div className="wake-word-main">
        <span className="wake-word-dot" />
        <div>
          <strong>Wake Word</strong>
          <p>{statusText}</p>
        </div>
      </div>
      <div className="wake-word-phrase" title={lastTranscript || status?.mode || ""}>
        {lastTranscript || status?.wakeWordPhrase || "jarvis"}
      </div>
      <button className="soft-button" type="button" onClick={toggleArmed}>
        {state === "checking" ? "..." : isArmed ? "Disarm" : "Arm"}
      </button>
    </section>
  );
}
