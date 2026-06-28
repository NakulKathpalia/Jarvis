import { SettingsCard } from "../SettingsCard";
import { InputRow } from "../SettingRows";
import type { SettingsDraftProps } from "../SettingsTypes";

export function WhisperPiperSettings({ draft, setDraft }: SettingsDraftProps) {
  return (
    <SettingsCard title="Whisper & Piper">
      <InputRow
        title="Whisper executable"
        text="Whisper executable path."
        value={draft.whisperExecutablePath}
        onChange={(value) =>
          setDraft({ ...draft, whisperExecutablePath: value })
        }
      />

      <InputRow
        title="Whisper model"
        text="Whisper model path."
        value={draft.whisperModelPath}
        onChange={(value) =>
          setDraft({ ...draft, whisperModelPath: value })
        }
      />

      <InputRow
        title="Whisper language"
        text="Recognition language."
        value={draft.whisperLanguage}
        onChange={(value) =>
          setDraft({ ...draft, whisperLanguage: value })
        }
      />

      <InputRow
        title="Piper executable"
        text="Piper executable path."
        value={draft.piperExecutablePath}
        onChange={(value) =>
          setDraft({ ...draft, piperExecutablePath: value })
        }
      />

      <InputRow
        title="Piper model"
        text="Piper model path."
        value={draft.piperModelPath}
        onChange={(value) =>
          setDraft({ ...draft, piperModelPath: value })
        }
      />
    </SettingsCard>
  );
}