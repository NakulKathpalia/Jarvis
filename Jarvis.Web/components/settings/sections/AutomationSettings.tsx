import { SettingsCard } from "../SettingsCard";
import { InputRow, ToggleRow } from "../SettingRows";
import type { SettingsDraftProps } from "../SettingsTypes";

export function AutomationSettings({
  draft,
  setDraft,
}: SettingsDraftProps) {
  return (
    <SettingsCard title="Automation">
      <ToggleRow
        title="Auto execute commands"
        text="Run safe commands automatically."
        checked={draft.autoExecuteCommands}
        onChange={(value) =>
          setDraft({
            ...draft,
            autoExecuteCommands: value,
          })
        }
      />

      <ToggleRow
        title="Auto speak assistant replies"
        text="Speak AI replies automatically."
        checked={draft.autoSpeakAssistantReplies}
        onChange={(value) =>
          setDraft({
            ...draft,
            autoSpeakAssistantReplies: value,
          })
        }
      />

      <ToggleRow
        title="Auto speak responses"
        text="Speak chat responses."
        checked={draft.autoSpeakResponses}
        onChange={(value) =>
          setDraft({
            ...draft,
            autoSpeakResponses: value,
          })
        }
      />

      <ToggleRow
        title="Enable wake word"
        text="Wake word detection."
        checked={draft.wakeWordEnabled}
        onChange={(value) =>
          setDraft({
            ...draft,
            wakeWordEnabled: value,
          })
        }
      />

      <InputRow
        title="Wake word"
        text="Wake phrase."
        value={draft.wakeWordPhrase}
        onChange={(value) =>
          setDraft({
            ...draft,
            wakeWordPhrase: value,
          })
        }
      />

      <InputRow
        title="Wake detector"
        text="Detector executable."
        value={draft.wakeWordDetectorPath}
        onChange={(value) =>
          setDraft({
            ...draft,
            wakeWordDetectorPath: value,
          })
        }
      />

      <InputRow
        title="Wake model"
        text="Model path."
        value={draft.wakeWordModelPath}
        onChange={(value) =>
          setDraft({
            ...draft,
            wakeWordModelPath: value,
          })
        }
      />
    </SettingsCard>
  );
}