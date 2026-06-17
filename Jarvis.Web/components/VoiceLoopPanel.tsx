"use client";

import { useEffect, useRef, useState } from "react";
import { jarvisApi } from "@/lib/api";
import { startVoiceCapture, type VoiceCaptureSession } from "@/lib/voiceCapture";

type VoiceLoopPanelProps = {
  disabled: boolean;
  autoSpeak: boolean;
  onSend: (message: string, options?: { skipAutoSpeak?: boolean }) => Promise<string>;
  onSpeak: (text: string) => Promise<void>;
  onToast: (message: string) => void;
  wakeSignal: number;
};

type VoiceLoopState = "idle" | "listening" | "transcribing" | "thinking" | "speaking" | "ready";
type VoiceLoopSource = "manual" | "wake";

export function VoiceLoopPanel({
  disabled,
  autoSpeak,
  onSend,
  onSpeak,
  onToast,
  wakeSignal
}: VoiceLoopPanelProps) {
  const [isActive, setIsActive] = useState(false);
  const [state, setState] = useState<VoiceLoopState>("idle");
  const [lastTranscript, setLastTranscript] = useState("");
  const captureRef = useRef<VoiceCaptureSession | null>(null);

  useEffect(() => {
    if (wakeSignal <= 0) {
      return;
    }

    void activateLoop("wake");
  }, [wakeSignal]);

  async function startLoop() {
    await activateLoop("manual");
  }

  async function activateLoop(source: VoiceLoopSource) {
    if (disabled) {
      onToast("Jarvis is busy.");
      return;
    }

    if (state === "listening" || state === "transcribing" || state === "thinking" || state === "speaking") {
      onToast("Voice loop is already active.");
      return;
    }

    setIsActive(true);
    await startListening(source);
  }

  function stopLoop() {
    captureRef.current?.cancel();
    captureRef.current = null;
    setIsActive(false);
    setState("idle");
    setLastTranscript("");
    onToast("Voice loop stopped");
  }

  async function startListening(source: VoiceLoopSource = "manual") {
    if (disabled) {
      return;
    }

    try {
      captureRef.current = await startVoiceCapture();
      setState("listening");
      onToast(source === "wake" ? "Wake word activated. Listening." : "Voice loop listening");
    } catch (error) {
      setIsActive(false);
      setState("idle");
      onToast(error instanceof Error ? error.message : "Microphone permission failed");
    }
  }

  async function finishTurn() {
    const capture = captureRef.current;
    if (!capture) {
      return;
    }

    captureRef.current = null;
    setState("transcribing");

    try {
      const wav = await capture.stop();
      const transcription = await jarvisApi.transcribeVoice(wav);
      const transcript = transcription.transcript.trim();

      if (!transcript) {
        onToast(transcription.message || "No transcript returned");
        setState(isActive ? "ready" : "idle");
        return;
      }

      setLastTranscript(transcript);
      setState("thinking");
      const response = await onSend(transcript, { skipAutoSpeak: true });

      if (autoSpeak && response.trim()) {
        setState("speaking");
        await onSpeak(response);
      }

      setState(isActive ? "ready" : "idle");
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Voice loop failed");
      setState(isActive ? "ready" : "idle");
    }
  }

  async function listenAgain() {
    await startListening("manual");
  }

  const statusText = getStatusText(state, isActive);

  return (
    <section className={isActive ? "voice-loop active" : "voice-loop"}>
      <div className="voice-loop-main">
        <span className="voice-loop-dot" />
        <div>
          <strong>Voice Loop</strong>
          <p>{statusText}</p>
        </div>
      </div>

      {lastTranscript && (
        <div className="voice-loop-transcript" title={lastTranscript}>
          {lastTranscript}
        </div>
      )}

      <div className="voice-loop-actions">
        {!isActive && (
          <button className="soft-button" type="button" disabled={disabled} onClick={startLoop}>
            Start loop
          </button>
        )}
        {isActive && state === "listening" && (
          <button className="danger-button" type="button" onClick={finishTurn}>
            Finish turn
          </button>
        )}
        {isActive && state === "ready" && (
          <button className="soft-button" type="button" disabled={disabled} onClick={listenAgain}>
            Listen again
          </button>
        )}
        {isActive && (
          <button className="soft-button" type="button" onClick={stopLoop}>
            Stop loop
          </button>
        )}
      </div>
    </section>
  );
}

function getStatusText(state: VoiceLoopState, isActive: boolean) {
  if (!isActive) {
    return "Start a hands-free turn.";
  }

  switch (state) {
    case "listening":
      return "Listening. Click Finish turn when done speaking.";
    case "transcribing":
      return "Transcribing local audio.";
    case "thinking":
      return "Sending transcript to Jarvis.";
    case "speaking":
      return "Speaking the response.";
    case "ready":
      return "Turn complete. Ready for the next one.";
    default:
      return "Voice loop ready.";
  }
}
