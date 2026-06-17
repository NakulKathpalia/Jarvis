import type { JarvisStatus, ViewKey } from "@/lib/types";
import { StatusCard } from "./StatusCard";

type SidebarProps = {
  activeView: ViewKey;
  onChangeView: (view: ViewKey) => void;
  status: JarvisStatus | null;
  memoryCount: number;
};

const navItems: Array<{ key: ViewKey; label: string; symbol: string }> = [
  { key: "chat", label: "Chat", symbol: "C" },
  { key: "memory", label: "Memory", symbol: "M" },
  { key: "files", label: "Files", symbol: "F" },
  { key: "settings", label: "Settings", symbol: "S" }
];

export function Sidebar({ activeView, onChangeView, status, memoryCount }: SidebarProps) {
  return (
    <aside className="sidebar">
      <div className="brand-row">
        <div className="brand-mark">J</div>
        <div>
          <h1>Jarvis</h1>
          <p>{status?.online ? "Ollama online" : "Local assistant"}</p>
        </div>
      </div>

      <button className="new-chat-button" type="button" onClick={() => onChangeView("chat")}>
        New chat
      </button>

      <nav className="side-nav" aria-label="Main navigation">
        {navItems.map((item) => (
          <button
            className={activeView === item.key ? "side-link active" : "side-link"}
            key={item.key}
            type="button"
            onClick={() => onChangeView(item.key)}
          >
            <span className="side-symbol">{item.symbol}</span>
            {item.label}
          </button>
        ))}
      </nav>

      <StatusCard status={status} memoryCount={memoryCount} />
    </aside>
  );
}
