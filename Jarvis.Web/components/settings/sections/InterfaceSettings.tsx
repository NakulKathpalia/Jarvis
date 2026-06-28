import { themeOptions } from "@/lib/theme";
import type { ThemeMode } from "@/lib/theme";
import type { AppSettings } from "@/lib/types";

import { SettingsCard } from "../SettingsCard";
import { SelectRow } from "../SettingRows";

type Props = {
  draft: AppSettings;
  setDraft: (value: AppSettings) => void;
  themeMode: ThemeMode;
  onThemeModeChange: (mode: ThemeMode) => void;
};

export function InterfaceSettings({
  draft,
  setDraft,
  themeMode,
  onThemeModeChange,
}: Props) {
  return (
    <SettingsCard title="Interface">
      <div className="settings-theme-options">
        {themeOptions.map((option) => (
          <label key={option.value}>
            <input
              type="radio"
              checked={themeMode === option.value}
              onChange={() => onThemeModeChange(option.value)}
            />
            <span>{option.label}</span>
          </label>
        ))}
      </div>

      <SelectRow
        title="Response Style"
        text="Assistant personality."
        value={draft.responseStyle}
        onChange={(value) =>
          setDraft({
            ...draft,
            responseStyle: value as AppSettings["responseStyle"],
          })
        }
        options={["Jarvis", "Neutral"]}
      />

      <SelectRow
        title="Preferred Language"
        text="Reply language."
        value={draft.preferredLanguage}
        onChange={(value) =>
          setDraft({
            ...draft,
            preferredLanguage:
              value as AppSettings["preferredLanguage"],
          })
        }
        options={["Auto", "English", "RomanHinglish"]}
      />

      <SelectRow
        title="Verbosity"
        text="Response detail."
        value={draft.verbosity}
        onChange={(value) =>
          setDraft({
            ...draft,
            verbosity: value as AppSettings["verbosity"],
          })
        }
        options={["Short", "Balanced", "Detailed"]}
      />
    </SettingsCard>
  );
}