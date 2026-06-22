"use client";

import { FormEvent, useEffect, useState } from "react";
import { jarvisApi } from "@/lib/api";
import { ThemeMode, themeOptions } from "@/lib/theme";
import type { AppSettings, DiagnosticsResult, VoiceStatus } from "@/lib/types";

type SettingsPanelProps = {
  settings: AppSettings;
  themeMode: ThemeMode;
  onThemeModeChange: (mode: ThemeMode) => void;
  onSave: (settings: AppSettings) => Promise<void>;
};

export function SettingsPanel({ settings, themeMode, onThemeModeChange, onSave }: SettingsPanelProps) {
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
      <div className="page-header">
        <h2>Settings</h2>
        <p>Local runtime configuration and appearance.</p>
      </div>

      <form className="settings-form" onSubmit={handleSubmit}>
        <div className="settings-section wide-field">
          <h3>Appearance</h3>
          <p>Choose how Jarvis should render the interface.</p>
          <div className="theme-radio-group">
            {themeOptions.map((option) => (
              <label key={option.value}>
                <input
                  checked={themeMode === option.value}
                  name="appearance-theme"
                  type="radio"
                  value={option.value}
                  onChange={() => onThemeModeChange(option.value)}
                />
                <span>{option.label}</span>
              </label>
            ))}
          </div>
        </div>

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
          <span>Ollama Context Length</span>
          <input
            type="number"
            min={512}
            max={32768}
            step={512}
            value={draft.ollamaContextLength || 8192}
            onChange={(event) =>
              setDraft({ ...draft, ollamaContextLength: Number.parseInt(event.target.value, 10) || 8192 })
            }
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

        <label className="toggle-field">
          <input
            checked={draft.memoryRetrievalEnabled}
            type="checkbox"
            onChange={(event) => setDraft({ ...draft, memoryRetrievalEnabled: event.target.checked })}
          />
          <span>Use relevant memories in assistant responses</span>
        </label>

        <label>
          <span>Retrieved Memories</span>
          <input
            type="number"
            min={1}
            max={10}
            value={draft.maxRetrievedMemories}
            onChange={(event) =>
              setDraft({ ...draft, maxRetrievedMemories: Number.parseInt(event.target.value, 10) || 5 })
            }
          />
        </label>

        <label className="toggle-field">
          <input
            checked={draft.useTemporaryContext}
            type="checkbox"
            onChange={(event) => setDraft({ ...draft, useTemporaryContext: event.target.checked })}
          />
          <span>Use unexpired temporary context</span>
        </label>

        <label className="toggle-field">
          <input
            checked={draft.useSuggestedMemories}
            type="checkbox"
            onChange={(event) => setDraft({ ...draft, useSuggestedMemories: event.target.checked })}
          />
          <span>Use approved suggested memories</span>
        </label>

        <label>
          <span>File Root</span>
          <input
            value={draft.fileIndexRoot}
            onChange={(event) => setDraft({ ...draft, fileIndexRoot: event.target.value })}
          />
        </label>

        <label>
          <span>Voice Mode</span>
          <select
            value={draft.voiceMode}
            onChange={(event) => setDraft({ ...draft, voiceMode: event.target.value as AppSettings["voiceMode"] })}
          >
            <option value="PushToTalk">Push to Talk</option>
            <option value="WakeWord">Wake Word (future)</option>
            <option value="AlwaysListening">Always Listening (future)</option>
            <option value="Hybrid">Hybrid (future)</option>
          </select>
        </label>

        <label>
          <span>Voice Language</span>
          <select
            value={draft.voiceLanguage}
            onChange={(event) => setDraft({ ...draft, voiceLanguage: event.target.value })}
          >
            <option value="en">English</option>
            <option value="hi">Hindi</option>
            <option value="auto">Hinglish / Auto</option>
          </select>
        </label>

        <label className="toggle-field">
          <input
            checked={draft.autoExecuteCommands}
            type="checkbox"
            onChange={(event) => setDraft({ ...draft, autoExecuteCommands: event.target.checked })}
          />
          <span>Auto-execute recognized safe commands</span>
        </label>

        <label className="toggle-field">
          <input
            checked={draft.noiseSuppression}
            type="checkbox"
            onChange={(event) => setDraft({ ...draft, noiseSuppression: event.target.checked })}
          />
          <span>Noise suppression preference</span>
        </label>

        <label>
          <span>Faster-Whisper Executable</span>
          <input
            placeholder="C:\\tools\\faster-whisper\\faster-whisper.exe"
            value={draft.whisperExecutablePath}
            onChange={(event) => setDraft({ ...draft, whisperExecutablePath: event.target.value })}
          />
        </label>

        <label>
          <span>Faster-Whisper Model</span>
          <input
            placeholder="large-v3 or C:\\models\\faster-whisper-large-v3"
            value={draft.whisperModelPath}
            onChange={(event) => setDraft({ ...draft, whisperModelPath: event.target.value })}
          />
        </label>

        <label>
          <span>Whisper Language</span>
          <input
            placeholder="legacy compatibility; use Voice Language above"
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
