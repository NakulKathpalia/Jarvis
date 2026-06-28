import { SettingsCard } from "../SettingsCard";
import { InputRow, SelectRow, ToggleRow } from "../SettingRows";
import type { AppSettings } from "@/lib/types";
import type { SettingsDraftProps } from "../SettingsTypes";

export function VoiceSettings({ draft, setDraft }: SettingsDraftProps) {
  return (
    <SettingsCard title="Voice & Speech">
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
    </SettingsCard>
  );
}