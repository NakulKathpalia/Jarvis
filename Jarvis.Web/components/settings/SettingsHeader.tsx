type SettingsHeaderProps = {
  onRestore: () => void;
};

export function SettingsHeader({ onRestore }: SettingsHeaderProps) {
  return (
    <div className="settings-top">
      <div>
        <h2>Settings</h2>
        <p>Manage how Jarvis behaves, remembers, and interacts with you.</p>
      </div>

      <button className="settings-restore" type="button" onClick={onRestore}>
        ↻ Restore loaded settings
      </button>
    </div>
  );
}