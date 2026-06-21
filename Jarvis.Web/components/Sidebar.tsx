"use client";

import type { ChatSessionSummary, JarvisStatus, ViewKey } from "@/lib/types";
import { useMemo, useState } from "react";

type SidebarProps = {
  activeView: ViewKey;
  activeChatId: string | null;
  chats: ChatSessionSummary[];
  onChangeView: (view: ViewKey) => void;
  onNewChat: () => Promise<void>;
  onOpenChat: (id: string) => Promise<void>;
  status: JarvisStatus | null;
  memoryCount: number;
};

const navSections: Array<{ title: string; items: Array<{ key: ViewKey; label: string; icon: string }> }> = [
  { title: "Main", items: [{ key: "chat", label: "Chat", icon: "C" }] }
];

export function Sidebar({
  activeView,
  activeChatId,
  chats,
  onChangeView,
  onNewChat,
  onOpenChat,
  status,
  memoryCount
}: SidebarProps) {
  const [query, setQuery] = useState("");
  const filteredChats = useMemo(() => {
    const normalized = query.trim().toLowerCase();
    if (!normalized) {
      return chats;
    }

    return chats.filter((chat) =>
      chat.title.toLowerCase().includes(normalized)
      || chat.preview.toLowerCase().includes(normalized)
    );
  }, [chats, query]);

  return (
    <aside className="sidebar">
      <div className="sidebar-header">
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

        <label className="sidebar-search">
          <span>Search Chats</span>
          <input
            value={query}
            placeholder="Search chats"
            onChange={(event) => setQuery(event.target.value)}
          />
        </label>

        <nav className="sidebar-nav" aria-label="Main navigation">
          {navSections.map((section) => (
            <div className="sidebar-nav-section" key={section.title}>
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
      </div>

      <section className="recent-chats">
        <div className="section-label">
          <span>Recent</span>
          <strong>{filteredChats.length}</strong>
        </div>
        {filteredChats.map((chat) => (
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
        <button className="sidebar-settings-button" type="button" onClick={() => onChangeView("tools")}>
          <span>S</span>
          Tools & Settings
        </button>
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
        <div className="sidebar-user-compact">
          <div className="user-avatar">U</div>
          <div>
            <strong>Local User</strong>
            <span>Placeholder account</span>
          </div>
        </div>
      </div>
    </aside>
  );
}
