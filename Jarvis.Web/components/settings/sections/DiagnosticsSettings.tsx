import { SettingsCard } from "../SettingsCard";
import { StatusLine } from "../SettingRows";
import type { StatusProps } from "../SettingsTypes";

export function DiagnosticsSettings({
  diagnostics,
}: StatusProps) {
  return (
    <SettingsCard title="Diagnostics">
      <StatusLine
        title="Platform"
        value={diagnostics?.platform ?? "Checking"}
      />

      <StatusLine
        title="App Data"
        value={diagnostics?.appDataPath ?? "Checking"}
      />

      <StatusLine
        title="Memory"
        value={diagnostics?.memoryPath ?? "Checking"}
      />

      <StatusLine
        title="Logs"
        value={diagnostics?.logsPath ?? "Checking"}
      />

      <StatusLine
        title="Ollama"
        value={diagnostics?.ollama.message ?? "Checking"}
      />
    </SettingsCard>
  );
}