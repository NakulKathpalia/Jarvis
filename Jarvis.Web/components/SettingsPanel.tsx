"use client";

import { FormEvent, ReactNode, useEffect, useState } from "react";
import { jarvisApi } from "@/lib/api";
import { ThemeMode, themeOptions } from "@/lib/theme";
import type { AppSettings, DiagnosticsResult, VoiceStatus } from "@/lib/types";

type SettingsPanelProps = {
  settings: AppSettings;
  themeMode: ThemeMode;
  onThemeModeChange: (mode: ThemeMode) => void;
  onSave: (settings: AppSettings) => Promise<void>;
};

const categories = [
  ["⌂", "Home"],
  ["▣", "System"],
  ["🎙", "Voice & Speech"],
  ["🧠", "Memory"],
  ["✧", "AI Model"],
  ["▢", "OCR"],
  ["▤", "Files & Knowledge"],
  ["◒", "Interface"],
  ["⚙", "Personalization"],
  ["◇", "Privacy & Security"],
  ["▣", "Automation"],
  ["☁", "Backup & Sync"],
  ["</>", "Advanced"],
  ["ⓘ", "About"],
];

export function SettingsPanel({ settings, themeMode, onThemeModeChange, onSave }: SettingsPanelProps) {
  const [draft, setDraft] = useState<AppSettings>(settings);
  const [isSaving, setIsSaving] = useState(false);
  const [voiceStatus, setVoiceStatus] = useState<VoiceStatus | null>(null);
  const [diagnostics, setDiagnostics] = useState<DiagnosticsResult | null>(null);

  useEffect(() => {
    setDraft(settings);
  }, [settings]);

  useEffect(() => {
    refreshStatus();
  }, []);

  async function refreshStatus() {
    try {
      const [voice, diag] = await Promise.all([
        jarvisApi.voiceStatus(),
        jarvisApi.diagnostics(),
      ]);
      setVoiceStatus(voice);
      setDiagnostics(diag);
    } catch {
      setVoiceStatus(null);
      setDiagnostics(null);
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSaving(true);
    try {
      const saved = await onSave(draft);
      await refreshStatus();
    } finally {
      setIsSaving(false);
    }
  }

  function restoreLoadedSettings() {
    setDraft(settings);
  }

  return (
    <section className="settings-page">
      <aside className="settings-leftbar">
        <div className="settings-brand">
          <span>J</span>
          <strong>Jarvis</strong>
        </div>

        <div className="settings-search">
          <input placeholder="Find a setting" />
          <span>⌕</span>
        </div>

        <nav className="settings-nav">
          {categories.map(([icon, label]) => (
            <button key={label} className={label === "Memory" ? "active" : ""} type="button">
              <span>{icon}</span>
              {label}
            </button>
          ))}
        </nav>

        <div className="settings-left-bottom">
          <button type="button" onClick={restoreLoadedSettings}>↺ Reset unsaved changes</button>
        </div>
      </aside>

      <main className="settings-main">
        <div className="settings-top">
          <div>
            <h2>Settings</h2>
            <p>Manage how Jarvis behaves, remembers, and interacts with you.</p>
          </div>

          <button className="settings-restore" type="button" onClick={restoreLoadedSettings}>
            ↻ Restore loaded settings
          </button>
        </div>

        <form className="settings-dashboard" onSubmit={handleSubmit}>
          <section className="quick-access">
            <h3>Quick Access</h3>
            <div className="quick-grid">
              <QuickCard icon="🧠" title="Memory" text="Memory retrieval and saved context" />
              <QuickCard icon="▤" title="Knowledge Library" text="Files and indexed knowledge" />
              <QuickCard icon="⇧" title="OCR" text="Image and document reading" />
              <QuickCard icon="≋" title="Voice" text="Speech and wake word" />
              <QuickCard icon="▭" title="Appearance" text="Theme and interface" />
            </div>
          </section>

          <div className="settings-card-grid">
            <Card title="Memory">
              <ToggleRow
                title="Use relevant memories"
                text="Jarvis can use saved memories in assistant replies."
                checked={draft.memoryRetrievalEnabled}
                onChange={(value) => setDraft({ ...draft, memoryRetrievalEnabled: value })}
              />

              <InputRow
                title="Retrieved memories"
                text="Maximum memories Jarvis can retrieve per response."
                type="number"
                value={draft.maxRetrievedMemories}
                onChange={(value) =>
                  setDraft({ ...draft, maxRetrievedMemories: Number.parseInt(value, 10) || 5 })
                }
              />

              <ToggleRow
                title="Use temporary context"
                text="Use unexpired temporary context while replying."
                checked={draft.useTemporaryContext}
                onChange={(value) => setDraft({ ...draft, useTemporaryContext: value })}
              />

              <ToggleRow
                title="Use suggested memories"
                text="Use approved suggested memories."
                checked={draft.useSuggestedMemories}
                onChange={(value) => setDraft({ ...draft, useSuggestedMemories: value })}
              />
            </Card>

            <Card title="Voice & Speech">
              <SelectRow
                title="Voice mode"
                text="Choose how Jarvis listens."
                value={draft.voiceMode}
                onChange={(value) => setDraft({ ...draft, voiceMode: value as AppSettings["voiceMode"] })}
                options={["PushToTalk", "WakeWord", "AlwaysListening", "Hybrid"]}
              />

              <SelectRow
                title="Voice language"
                text="Speech recognition language."
                value={draft.voiceLanguage}
                onChange={(value) => setDraft({ ...draft, voiceLanguage: value })}
                options={["en", "hi", "auto"]}
              />

              <ToggleRow
                title="Noise suppression"
                text="Prefer noise suppression for microphone input."
                checked={draft.noiseSuppression}
                onChange={(value) => setDraft({ ...draft, noiseSuppression: value })}
              />

              <ToggleRow
                title="Enable voice responses"
                text="Allow Jarvis to speak responses."
                checked={draft.enableVoiceResponses}
                onChange={(value) => setDraft({ ...draft, enableVoiceResponses: value })}
              />

              <InputRow
                title="Voice name"
                text="TTS voice name."
                value={draft.voiceName}
                onChange={(value) => setDraft({ ...draft, voiceName: value })}
              />

              <InputRow
                title="Speech rate"
                text="Voice speed."
                type="number"
                value={draft.speechRate}
                onChange={(value) => setDraft({ ...draft, speechRate: Number(value) })}
              />

              <InputRow
                title="Speech volume"
                text="Voice volume."
                type="number"
                value={draft.speechVolume}
                onChange={(value) => setDraft({ ...draft, speechVolume: Number(value) })}
              />
            </Card>

            <Card title="AI Model">
              <InputRow
                title="Ollama URL"
                text="Local Ollama server address."
                value={draft.ollamaBaseUrl}
                onChange={(value) => setDraft({ ...draft, ollamaBaseUrl: value })}
              />

              <InputRow
                title="Model"
                text="Model Jarvis should use."
                value={draft.model}
                onChange={(value) => setDraft({ ...draft, model: value })}
              />

              <InputRow
                title="Ollama context length"
                text="Maximum context length."
                type="number"
                value={draft.ollamaContextLength}
                onChange={(value) =>
                  setDraft({ ...draft, ollamaContextLength: Number.parseInt(value, 10) || 8192 })
                }
              />

              <InputRow
                title="History messages"
                text="Recent messages sent with prompt."
                type="number"
                value={draft.maxHistoryMessages}
                onChange={(value) =>
                  setDraft({ ...draft, maxHistoryMessages: Number.parseInt(value, 10) || 1 })
                }
              />
            </Card>

            <Card title="Files & Knowledge">
              <InputRow
                title="File root"
                text="Folder Jarvis uses for file search and indexing."
                value={draft.fileIndexRoot}
                onChange={(value) => setDraft({ ...draft, fileIndexRoot: value })}
              />

              <InputRow
                title="Tesseract executable"
                text="OCR executable path."
                value={draft.tesseractExecutablePath}
                onChange={(value) => setDraft({ ...draft, tesseractExecutablePath: value })}
              />

              <InputRow
                title="Tesseract language"
                text="OCR language pack setting."
                value={draft.tesseractLanguage}
                onChange={(value) => setDraft({ ...draft, tesseractLanguage: value })}
              />

              <StatusLine title="OCR status" value={voiceStatus?.ocr.message ?? "Checking"} />
            </Card>

            <Card title="Whisper & Piper">
              <InputRow
                title="Whisper executable"
                text="Faster-Whisper executable path."
                value={draft.whisperExecutablePath}
                onChange={(value) => setDraft({ ...draft, whisperExecutablePath: value })}
              />

              <InputRow
                title="Whisper model"
                text="Faster-Whisper model path or name."
                value={draft.whisperModelPath}
                onChange={(value) => setDraft({ ...draft, whisperModelPath: value })}
              />

              <InputRow
                title="Whisper language"
                text="Legacy compatibility language."
                value={draft.whisperLanguage}
                onChange={(value) => setDraft({ ...draft, whisperLanguage: value })}
              />

              <InputRow
                title="Piper executable"
                text="Piper executable path."
                value={draft.piperExecutablePath}
                onChange={(value) => setDraft({ ...draft, piperExecutablePath: value })}
              />

              <InputRow
                title="Piper model"
                text="Piper model path."
                value={draft.piperModelPath}
                onChange={(value) => setDraft({ ...draft, piperModelPath: value })}
              />
            </Card>

            <Card title="Interface">
              <div className="settings-theme-options">
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

              <SelectRow
                title="Response style"
                text="Assistant personality style."
                value={draft.responseStyle}
                onChange={(value) => setDraft({ ...draft, responseStyle: value as AppSettings["responseStyle"] })}
                options={["Jarvis", "Neutral"]}
              />

              <SelectRow
                title="Preferred language"
                text="Assistant reply language."
                value={draft.preferredLanguage}
                onChange={(value) =>
                  setDraft({ ...draft, preferredLanguage: value as AppSettings["preferredLanguage"] })
                }
                options={["Auto", "English", "RomanHinglish"]}
              />

              <SelectRow
                title="Verbosity"
                text="Assistant answer length."
                value={draft.verbosity}
                onChange={(value) => setDraft({ ...draft, verbosity: value as AppSettings["verbosity"] })}
                options={["Short", "Balanced", "Detailed"]}
              />
            </Card>

            <Card title="Automation & Wake Word">
              <ToggleRow
                title="Auto-execute safe commands"
                text="Run recognized safe commands automatically."
                checked={draft.autoExecuteCommands}
                onChange={(value) => setDraft({ ...draft, autoExecuteCommands: value })}
              />

              <ToggleRow
                title="Auto-speak assistant replies"
                text="Speak assistant replies automatically."
                checked={draft.autoSpeakAssistantReplies}
                onChange={(value) => setDraft({ ...draft, autoSpeakAssistantReplies: value })}
              />

              <ToggleRow
                title="Auto-speak chat responses"
                text="Speak chat responses automatically."
                checked={draft.autoSpeakResponses}
                onChange={(value) => setDraft({ ...draft, autoSpeakResponses: value })}
              />

              <ToggleRow
                title="Enable wake word"
                text="Allow wake word detection."
                checked={draft.wakeWordEnabled}
                onChange={(value) => setDraft({ ...draft, wakeWordEnabled: value })}
              />

              <InputRow
                title="Wake word phrase"
                text="Phrase used to wake Jarvis."
                value={draft.wakeWordPhrase}
                onChange={(value) => setDraft({ ...draft, wakeWordPhrase: value })}
              />

              <InputRow
                title="Wake detector"
                text="Wake word detector executable path."
                value={draft.wakeWordDetectorPath}
                onChange={(value) => setDraft({ ...draft, wakeWordDetectorPath: value })}
              />

              <InputRow
                title="Wake model"
                text="Wake word model path."
                value={draft.wakeWordModelPath}
                onChange={(value) => setDraft({ ...draft, wakeWordModelPath: value })}
              />
            </Card>

            <Card title="Diagnostics">
              <StatusLine title="Platform" value={diagnostics?.platform ?? "Checking"} />
              <StatusLine title="App Data" value={diagnostics?.appDataPath ?? "Checking"} />
              <StatusLine title="Memory" value={diagnostics?.memoryPath ?? "Checking"} />
              <StatusLine title="Logs" value={diagnostics?.logsPath ?? "Checking"} />
              <StatusLine title="Screenshots" value={diagnostics?.screenshotPath ?? "Checking"} />
              <StatusLine title="Audio" value={diagnostics?.generatedAudioPath ?? "Checking"} />
              <StatusLine title="Ollama" value={diagnostics?.ollama.message ?? "Checking"} />
              <StatusLine title="Whisper" value={diagnostics?.whisper.message ?? "Checking"} />
              <StatusLine title="Piper" value={diagnostics?.piper.message ?? "Checking"} />
            </Card>

            <label className="settings-prompt-card">
              <span>System Prompt</span>
              <textarea
                rows={6}
                value={draft.systemPrompt}
                onChange={(event) => setDraft({ ...draft, systemPrompt: event.target.value })}
              />
            </label>
          </div>

          <button className="settings-save" type="submit" disabled={isSaving}>
            {isSaving ? "Saving..." : "Save settings"}
          </button>
        </form>
      </main>
    </section>
  );
}

function QuickCard({ icon, title, text }: { icon: string; title: string; text: string }) {
  return (
    <button className="quick-card" type="button">
      <span>{icon}</span>
      <div>
        <strong>{title}</strong>
        <p>{text}</p>
      </div>
      <b>›</b>
    </button>
  );
}

function Card({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="settings-glass-card">
      <h3>{title}</h3>
      <div>{children}</div>
    </section>
  );
}

function ToggleRow({
  title,
  text,
  checked,
  onChange,
}: {
  title: string;
  text: string;
  checked: boolean;
  onChange: (value: boolean) => void;
}) {
  return (
    <div className="setting-row">
      <div>
        <strong>{title}</strong>
        <p>{text}</p>
      </div>
      <label className="jarvis-switch">
        <input checked={checked} type="checkbox" onChange={(event) => onChange(event.target.checked)} />
        <span />
      </label>
    </div>
  );
}

function InputRow({
  title,
  text,
  value,
  onChange,
  type = "text",
}: {
  title: string;
  text: string;
  value: string | number;
  onChange: (value: string) => void;
  type?: string;
}) {
  return (
    <label className="setting-row">
      <div>
        <strong>{title}</strong>
        {text && <p>{text}</p>}
      </div>
      <input type={type} value={value ?? ""} onChange={(event) => onChange(event.target.value)} />
    </label>
  );
}

function SelectRow({
  title,
  text,
  value,
  onChange,
  options,
}: {
  title: string;
  text: string;
  value: string;
  onChange: (value: string) => void;
  options: string[];
}) {
  return (
    <label className="setting-row">
      <div>
        <strong>{title}</strong>
        <p>{text}</p>
      </div>
      <select value={value} onChange={(event) => onChange(event.target.value)}>
        {options.map((option) => (
          <option key={option} value={option}>
            {option}
          </option>
        ))}
      </select>
    </label>
  );
}

function StatusLine({ title, value }: { title: string; value: string }) {
  return (
    <div className="setting-row">
      <div>
        <strong>{title}</strong>
        <p>{value}</p>
      </div>
    </div>
  );
}