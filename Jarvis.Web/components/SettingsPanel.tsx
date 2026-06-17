"use client";

import { FormEvent, useEffect, useState } from "react";
import { TopBar } from "./TopBar";
import { jarvisApi } from "@/lib/api";
import type { AppSettings, DiagnosticsResult, VoiceStatus } from "@/lib/types";

type SettingsPanelProps = {
  settings: AppSettings;
  onSave: (settings: AppSettings) => Promise<void>;
};

export function SettingsPanel({ settings, onSave }: SettingsPanelProps) {
  const [draft, setDraft] = useState<AppSettings>(settings);
  const [isSaving, setIsSaving] = useState(false);
  const [voiceStatus, setVoiceStatus] = useState<VoiceStatus | null>(null);
  const [diagnostics, setDiagnostics] = useState<DiagnosticsResult | null>(null);

  useEffect(() => {
    jarvisApi.voiceStatus().then(setVoiceStatus).catch(() => setVoiceStatus(null));
    jarvisApi.diagnostics().then(setDiagnostics).catch(() => setDiagnostics(null));
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);
    await onSave(draft);
    setVoiceStatus(await jarvisApi.voiceStatus());
    setDiagnostics(await jarvisApi.diagnostics());
    setIsSaving(false);
  }

  return (
    <section className="tool-panel">
      <TopBar title="Settings" subtitle="Local runtime configuration" />

      <form className="settings-form" onSubmit={handleSubmit}>
        <div className="voice-setup-card wide-field">
          <div className="voice-setup-row">
            <strong>Whisper</strong>
            <span className={voiceStatus?.whisper.configured ? "online" : "offline"}>
              {voiceStatus?.whisper.message ?? "Checking"}
            </span>
          </div>
          <div className="voice-setup-row">
            <strong>Piper</strong>
            <span className={voiceStatus?.piper.configured ? "online" : "offline"}>
              {voiceStatus?.piper.message ?? "Checking"}
            </span>
          </div>
          <div className="voice-setup-row">
            <strong>Wake Word</strong>
            <span className={voiceStatus?.wakeWord.configured ? "online" : "offline"}>
              {voiceStatus?.wakeWord.message ?? "Checking"}
            </span>
          </div>
        </div>

        {diagnostics && (
          <div className="diagnostics-card wide-field">
            <div className="voice-setup-row">
              <strong>Platform</strong>
              <span>{diagnostics.platform}</span>
            </div>
            <div className="voice-setup-row">
              <strong>App Data</strong>
              <span>{diagnostics.appDataPath}</span>
            </div>
            <div className="voice-setup-row">
              <strong>Memory</strong>
              <span>{diagnostics.memoryPath}</span>
            </div>
            <div className="voice-setup-row">
              <strong>Logs</strong>
              <span>{diagnostics.logsPath}</span>
            </div>
            <div className="voice-setup-row">
              <strong>Screenshots</strong>
              <span>{diagnostics.screenshotPath}</span>
            </div>
            <div className="voice-setup-row">
              <strong>Audio</strong>
              <span>{diagnostics.generatedAudioPath}</span>
            </div>
            <div className="diagnostics-health">
              <span className={diagnostics.ollama.healthy ? "online" : "offline"}>Ollama: {diagnostics.ollama.message}</span>
              <span className={diagnostics.whisper.healthy ? "online" : "offline"}>Whisper: {diagnostics.whisper.message}</span>
              <span className={diagnostics.piper.healthy ? "online" : "offline"}>Piper: {diagnostics.piper.message}</span>
            </div>
            {diagnostics.warnings.length > 0 && (
              <div className="diagnostics-warnings">
                {diagnostics.warnings.map((warning) => (
                  <span key={warning}>{warning}</span>
                ))}
              </div>
            )}
          </div>
        )}

        <label>
          <span>Ollama URL</span>
          <input
            value={draft.ollamaBaseUrl}
            onChange={(event) => setDraft({ ...draft, ollamaBaseUrl: event.target.value })}
          />
        </label>

        <label>
          <span>Model</span>
          <input
            value={draft.model}
            onChange={(event) => setDraft({ ...draft, model: event.target.value })}
          />
        </label>

        <label>
          <span>History Messages</span>
          <input
            type="number"
            min={1}
            value={draft.maxHistoryMessages}
            onChange={(event) =>
              setDraft({ ...draft, maxHistoryMessages: Number.parseInt(event.target.value, 10) || 1 })
            }
          />
        </label>

        <label>
          <span>File Root</span>
          <input
            value={draft.fileIndexRoot}
            onChange={(event) => setDraft({ ...draft, fileIndexRoot: event.target.value })}
          />
        </label>

        <label>
          <span>Whisper Executable</span>
          <input
            placeholder="C:\\tools\\whisper.cpp\\build\\bin\\Release\\whisper-cli.exe"
            value={draft.whisperExecutablePath}
            onChange={(event) => setDraft({ ...draft, whisperExecutablePath: event.target.value })}
          />
        </label>

        <label>
          <span>Whisper Model</span>
          <input
            placeholder="C:\\models\\ggml-base.en.bin"
            value={draft.whisperModelPath}
            onChange={(event) => setDraft({ ...draft, whisperModelPath: event.target.value })}
          />
        </label>

        <label>
          <span>Whisper Language</span>
          <input
            placeholder="auto"
            value={draft.whisperLanguage}
            onChange={(event) => setDraft({ ...draft, whisperLanguage: event.target.value })}
          />
        </label>

        <label>
          <span>Piper Executable</span>
          <input
            placeholder="C:\\tools\\piper\\piper.exe"
            value={draft.piperExecutablePath}
            onChange={(event) => setDraft({ ...draft, piperExecutablePath: event.target.value })}
          />
        </label>

        <label>
          <span>Piper Model</span>
          <input
            placeholder="C:\\models\\en_US-lessac-medium.onnx"
            value={draft.piperModelPath}
            onChange={(event) => setDraft({ ...draft, piperModelPath: event.target.value })}
          />
        </label>

        <label className="toggle-field">
          <input
            checked={draft.autoSpeakResponses}
            type="checkbox"
            onChange={(event) => setDraft({ ...draft, autoSpeakResponses: event.target.checked })}
          />
          <span>Auto-speak assistant responses</span>
        </label>

        <label className="toggle-field">
          <input
            checked={draft.wakeWordEnabled}
            type="checkbox"
            onChange={(event) => setDraft({ ...draft, wakeWordEnabled: event.target.checked })}
          />
          <span>Enable wake word</span>
        </label>

        <label>
          <span>Wake Word Phrase</span>
          <input
            placeholder="jarvis"
            value={draft.wakeWordPhrase}
            onChange={(event) => setDraft({ ...draft, wakeWordPhrase: event.target.value })}
          />
        </label>

        <label>
          <span>Wake Detector</span>
          <input
            placeholder="C:\\tools\\wakeword\\detector.exe"
            value={draft.wakeWordDetectorPath}
            onChange={(event) => setDraft({ ...draft, wakeWordDetectorPath: event.target.value })}
          />
        </label>

        <label>
          <span>Wake Model</span>
          <input
            placeholder="C:\\models\\jarvis-wake.onnx"
            value={draft.wakeWordModelPath}
            onChange={(event) => setDraft({ ...draft, wakeWordModelPath: event.target.value })}
          />
        </label>

        <label className="wide-field">
          <span>System Prompt</span>
          <textarea
            rows={6}
            value={draft.systemPrompt}
            onChange={(event) => setDraft({ ...draft, systemPrompt: event.target.value })}
          />
        </label>

        <button className="primary-button" type="submit" disabled={isSaving}>
          {isSaving ? "Saving" : "Save settings"}
        </button>
      </form>
    </section>
  );
}
