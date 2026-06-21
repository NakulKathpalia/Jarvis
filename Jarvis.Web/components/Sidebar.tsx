"use client";

import { ThemeMode, themeOptions } from "@/lib/theme";
import type { ChatSessionSummary, JarvisStatus, ViewKey } from "@/lib/types";

type SidebarProps = {
  activeView: ViewKey;
  activeChatId: string | null;
  chats: ChatSessionSummary[];
  onChangeView: (view: ViewKey) => void;
  onNewChat: () => Promise<void>;
  onOpenChat: (id: string) => Promise<void>;
  status: JarvisStatus | null;
  memoryCount: number;
  themeMode: ThemeMode;
  onThemeModeChange: (mode: ThemeMode) => void;
};

const navSections: Array<{ title: string; items: Array<{ key: ViewKey; label: string; icon: string }> }> = [
  {
    title: "Main",
    items: [
      { key: "chat", label: "Chat", icon: "C" },
      { key: "voice", label: "Voice", icon: "V" },
      { key: "memory", label: "Memory", icon: "M" },
      { key: "control", label: "PC Control", icon: "P" },
      { key: "files", label: "Files", icon: "F" }
    ]
  },
  {
    title: "Platform",
    items: [
      { key: "auth", label: "Auth", icon: "A" },
      { key: "connectedApps", label: "Connected Apps", icon: "L" },
      { key: "security", label: "Security", icon: "S" }
    ]
  },
  {
    title: "System",
    items: [
      { key: "settings", label: "Settings", icon: "G" },
      { key: "diagnostics", label: "Diagnostics", icon: "D" },
      { key: "activity", label: "Activity", icon: "Y" }
    ]
  }
];

export function Sidebar({
  activeView,
  activeChatId,
  chats,
  onChangeView,
  onNewChat,
  onOpenChat,
  status,
  memoryCount,
  themeMode,
  onThemeModeChange
}: SidebarProps) {
  return (
    <aside className="sidebar">
      <div className="brand-row">
        <div className="jarvis-logo">J</div>
        <div>
          <h1>JARVIS</h1>
          <p>Personal AI Assistant</p>
        </div>
      </div>

      <button className="new-chat-button" type="button" onClick={() => void onNewChat()}>
        + New Chat
      </button>

      <nav className="sidebar-nav" aria-label="Main navigation">
        {navSections.map((section) => (
          <div className="sidebar-nav-section" key={section.title}>
            <div className="sidebar-nav-label">{section.title}</div>
            {section.items.map((item) => (
              <button
                className={activeView === item.key ? "active" : ""}
                key={item.key}
                type="button"
                onClick={() => onChangeView(item.key)}
              >
                <span>{item.icon}</span>
                {item.label}
              </button>
            ))}
          </div>
        ))}
      </nav>

      <section className="recent-chats">
        <div className="section-label">
          <span>Recent</span>
          <strong>{chats.length}</strong>
        </div>
        {chats.slice(0, 5).map((chat) => (
          <button
            className={chat.id === activeChatId ? "chat-link active" : "chat-link"}
            key={chat.id}
            type="button"
            onClick={() => void onOpenChat(chat.id)}
          >
            <strong>{chat.title}</strong>
            <span>{chat.preview || "New conversation"}</span>
          </button>
        ))}
      </section>

      <div className="sidebar-bottom">
        <div className="status-pill">
          <span className={status?.online ? "dot online" : "dot"} />
          Local AI
        </div>
        <div className="mini-stats">
          <span>Model</span>
          <strong title={status?.model ?? ""}>{status?.model ?? "Offline"}</strong>
        </div>
        <div className="mini-stats">
          <span>Memory</span>
          <strong>{memoryCount}</strong>
        </div>
        <fieldset className="theme-fieldset">
          <legend>Theme</legend>
          {themeOptions.map((option) => (
            <label key={option.value}>
              <input
                checked={themeMode === option.value}
                name="theme"
                type="radio"
                value={option.value}
                onChange={() => onThemeModeChange(option.value)}
              />
              {option.value === ThemeMode.System ? "System" : option.label}
            </label>
          ))}
        </fieldset>
      </div>
    </aside>
  );
}
