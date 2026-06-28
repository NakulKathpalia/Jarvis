import { SettingsCard } from "../SettingsCard";
import { InputRow, StatusLine } from "../SettingRows";
import type { SettingsDraftProps, StatusProps } from "../SettingsTypes";

type FilesKnowledgeSettingsProps = SettingsDraftProps & StatusProps;

export function FilesKnowledgeSettings({ draft, setDraft, voiceStatus }: FilesKnowledgeSettingsProps) {
  return (
    <SettingsCard title="Files & Knowledge">
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
    </SettingsCard>
  );
}