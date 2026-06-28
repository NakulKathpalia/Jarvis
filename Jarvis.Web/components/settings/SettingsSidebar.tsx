import { settingsCategories } from "./SettingsData";

type SettingsSidebarProps = {
  onRestore: () => void;
};

export function SettingsSidebar({ onRestore }: SettingsSidebarProps) {
  return (
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
        {settingsCategories.map(([icon, label]) => (
          <button key={label} className={label === "Memory" ? "active" : ""} type="button">
            <span>{icon}</span>
            {label}
          </button>
        ))}
      </nav>

      <div className="settings-left-bottom">
        <button type="button" onClick={onRestore}>
          ↺ Reset unsaved changes
        </button>
      </div>
    </aside>
  );
}