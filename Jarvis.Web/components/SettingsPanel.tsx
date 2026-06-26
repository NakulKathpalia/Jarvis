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

  function resetDefaults() {
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
          <button type="button" onClick={resetDefaults}>↺ Reset to defaults</button>
          <button type="button" onClick={() => onThemeModeChange(themeMode === "dark" ? "light" : "dark")}>
            ◐ Dark mode
          </button>
        </div>
      </aside>

      <main className="settings-main">
        <div className="settings-top">
          <div>
            <h2>Settings</h2>
            <p>Manage how Jarvis behaves, remembers, and interacts with you.</p>
          </div>
          <button className="settings-restore" type="button" onClick={resetDefaults}>
            ↻ Restore defaults
          </button>
        </div>

        <form className="settings-dashboard" onSubmit={handleSubmit}>
          <section className="quick-access">
            <h3>Quick Access</h3>
            <div className="quick-grid">
              <QuickCard icon="🧠" title="Memory" text="View and manage memories" />
              <QuickCard icon="▤" title="Knowledge Library" text="Browse your knowledge base" />
              <QuickCard icon="⇧" title="Imports" text="Review and manage imports" />
              <QuickCard icon="≋" title="Voice" text="Configure voice settings" />
              <QuickCard icon="▭" title="Appearance" text="Customize look and feel" />
            </div>
          </section>

          <div className="settings-card-grid">
            <Card title="Memory">
              <ToggleRow
                title="Auto-save important conversations"
                text="Jarvis will automatically save important information to memory."
                checked={draft.memoryRetrievalEnabled}
                onChange={(value) => setDraft({ ...draft, memoryRetrievalEnabled: value })}
              />
              <ToggleRow
                title="Memory review"
                text="Review suggested memories before saving."
                checked={draft.useSuggestedMemories}
                onChange={(value) => setDraft({ ...draft, useSuggestedMemories: value })}
              />
              <SelectRow
                title="Memory retention"
                text="Choose how long temporary memories are kept."
                value={draft.useTemporaryContext ? "30 days" : "Off"}
                onChange={(value) => setDraft({ ...draft, useTemporaryContext: value !== "Off" })}
                options={["Off", "7 days", "30 days", "90 days"]}
              />
              <InputRow
                title="Maximum memories"
                text="Limit the number of memories stored."
                type="number"
                value={draft.maxRetrievedMemories}
                onChange={(value) => setDraft({ ...draft, maxRetrievedMemories: Number.parseInt(value, 10) || 5 })}
              />
              <ActionRow title="Manage memory" text="View, edit or delete memories." button="Open Memory" />
            </Card>

            <Card title="Voice & Speech">
              <SelectRow
                title="Voice input (Speech-to-Text)"
                text={voiceStatus?.whisper.message ?? "Checking"}
                value="Whisper (Local)"
                onChange={() => undefined}
                options={["Whisper (Local)"]}
              />
              <SelectRow
                title="Voice output (Text-to-Speech)"
                text={voiceStatus?.piper.message ?? "Checking"}
                value="edge-tts (Local)"
                onChange={() => undefined}
                options={["edge-tts (Local)", "Piper (Local)"]}
              />
              <ToggleRow
                title="Wake word"
                text="Enable wake word detection."
                checked={draft.wakeWordEnabled}
                onChange={(value) => setDraft({ ...draft, wakeWordEnabled: value })}
              />
              <InputRow
                title="Wake word phrase"
                text=""
                value={draft.wakeWordPhrase}
                onChange={(value) => setDraft({ ...draft, wakeWordPhrase: value })}
              />
              <ToggleRow
                title="Auto listen"
                text="Automatically start listening."
                checked={draft.voiceMode === "AlwaysListening"}
                onChange={(value) =>
                  setDraft({ ...draft, voiceMode: value ? "AlwaysListening" : "PushToTalk" })
                }
              />
              <ActionRow title="" text="" button="Configure Voice" />
            </Card>

            <Card title="AI Model">
              <InputRow
                title="Provider"
                text="Select the AI model provider."
                value="Ollama (Local)"
                onChange={() => undefined}
              />
              <InputRow
                title="Model"
                text="Select the model Jarvis should use."
                value={draft.model}
                onChange={(value) => setDraft({ ...draft, model: value })}
              />
              <InputRow
                title="Ollama URL"
                text="Local Ollama server address."
                value={draft.ollamaBaseUrl}
                onChange={(value) => setDraft({ ...draft, ollamaBaseUrl: value })}
              />
              <InputRow
                title="Context length"
                text="Maximum context Jarvis can use."
                type="number"
                value={draft.ollamaContextLength || 8192}
                onChange={(value) =>
                  setDraft({ ...draft, ollamaContextLength: Number.parseInt(value, 10) || 8192 })
                }
              />
              <InputRow
                title="History messages"
                text="Recent messages sent to model."
                type="number"
                value={draft.maxHistoryMessages}
                onChange={(value) =>
                  setDraft({ ...draft, maxHistoryMessages: Number.parseInt(value, 10) || 1 })
                }
              />
            </Card>

            <Card title="Files & Knowledge">
              <InputRow
                title="Knowledge base location"
                text="Folder Jarvis uses for indexing."
                value={draft.fileIndexRoot}
                onChange={(value) => setDraft({ ...draft, fileIndexRoot: value })}
              />
              <ToggleRow
                title="Auto index new files"
                text="Automatically index new files and documents."
                checked={true}
                onChange={() => undefined}
              />
              <ActionRow title="Supported file types" text="pdf, docx, txt, md, jpg, png, webp" button="Manage" />
              <ActionRow title="Re-index all" text="Rebuild the entire knowledge index." button="Re-index" />
            </Card>

            <Card title="Appearance">
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
            </Card>

            <Card title="Advanced">
              <SelectRow
                title="Voice language"
                text="Language used for recognition."
                value={draft.voiceLanguage}
                onChange={(value) => setDraft({ ...draft, voiceLanguage: value })}
                options={["en", "hi", "auto"]}
              />
              <SelectRow
                title="Preferred language"
                text="Language style for replies."
                value={draft.preferredLanguage ?? "Auto"}
                onChange={(value) =>
                  setDraft({ ...draft, preferredLanguage: value as AppSettings["preferredLanguage"] })
                }
                options={["Auto", "English", "RomanHinglish"]}
              />
              <SelectRow
                title="Verbosity"
                text="Response detail level."
                value={draft.verbosity ?? "Short"}
                onChange={(value) => setDraft({ ...draft, verbosity: value as AppSettings["verbosity"] })}
                options={["Short", "Balanced", "Detailed"]}
              />
              <ToggleRow
                title="Auto-execute safe commands"
                text="Run recognized safe commands automatically."
                checked={draft.autoExecuteCommands}
                onChange={(value) => setDraft({ ...draft, autoExecuteCommands: value })}
              />
              <ToggleRow
                title="Voice responses"
                text="Enable spoken Jarvis responses."
                checked={draft.enableVoiceResponses}
                onChange={(value) => setDraft({ ...draft, enableVoiceResponses: value })}
              />
            </Card>

            <Card title="Diagnostics">
              <StatusLine title="Platform" value={diagnostics?.platform ?? "Checking"} />
              <StatusLine title="App Data" value={diagnostics?.appDataPath ?? "Checking"} />
              <StatusLine title="Memory" value={diagnostics?.memoryPath ?? "Checking"} />
              <StatusLine title="Logs" value={diagnostics?.logsPath ?? "Checking"} />
              <StatusLine title="Ollama" value={diagnostics?.ollama.message ?? "Checking"} />
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

function Card({ title, children }: { title: string; children: React.ReactNode }) {
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
      <input type={type} value={value} onChange={(event) => onChange(event.target.value)} />
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
          <option key={option} value={option}>{option}</option>
        ))}
      </select>
    </label>
  );
}

function ActionRow({ title, text, button }: { title: string; text: string; button: string }) {
  return (
    <div className="setting-row">
      <div>
        {title && <strong>{title}</strong>}
        {text && <p>{text}</p>}
      </div>
      <button className="small-action" type="button">{button}</button>
    </div>
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