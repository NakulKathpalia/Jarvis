import { SettingsCard } from "../SettingsCard";
import { InputRow } from "../SettingRows";
import type { SettingsDraftProps } from "../SettingsTypes";

export function AiModelSettings({ draft, setDraft }: SettingsDraftProps) {
  return (
    <SettingsCard title="AI Model">
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
    </SettingsCard>
  );
}