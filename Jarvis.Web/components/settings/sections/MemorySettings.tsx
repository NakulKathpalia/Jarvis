import { SettingsCard } from "../SettingsCard";
import { InputRow, ToggleRow } from "../SettingRows";
import type { SettingsDraftProps } from "../SettingsTypes";

export function MemorySettings({ draft, setDraft }: SettingsDraftProps) {
  return (
    <SettingsCard title="Memory">
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
    </SettingsCard>
  );
}