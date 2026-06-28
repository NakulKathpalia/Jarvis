import type { AppSettings, DiagnosticsResult, VoiceStatus } from "@/lib/types";
import type { ThemeMode } from "@/lib/theme";

export type SettingsPanelProps = {
  settings: AppSettings;
  themeMode: ThemeMode;
  onThemeModeChange: (mode: ThemeMode) => void;
  onSave: (settings: AppSettings) => Promise<void>;
};

export type SettingsDraftProps = {
  draft: AppSettings;
  setDraft: (settings: AppSettings) => void;
};

export type StatusProps = {
  voiceStatus: VoiceStatus | null;
  diagnostics: DiagnosticsResult | null;
};