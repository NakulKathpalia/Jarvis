"use client";

import { useState } from "react";
import { AssistantOrb } from "./ui/AssistantOrb";
import { StatusBadge } from "./ui/StatusBadge";
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
};

const navItems: Array<{ key: ViewKey; label: string; symbol: string }> = [
  { key: "chat", label: "Chat", symbol: "C" },
  { key: "memory", label: "Memory", symbol: "M" },
  { key: "files", label: "Files", symbol: "F" },
  { key: "control", label: "Control", symbol: "P" },
  { key: "voice", label: "Voice", symbol: "V" },
  { key: "activity", label: "Activity", symbol: "A" },
  { key: "settings", label: "Settings", symbol: "S" },
  { key: "diagnostics", label: "Diagnostics", symbol: "D" }
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
  const [isOpen, setIsOpen] = useState(false);

  async function handleOpenChat(id: string) {
    await onOpenChat(id);
    setIsOpen(false);
  }

  async function handleNewChat() {
    await onNewChat();
    setIsOpen(false);
  }

  function handleNav(view: ViewKey) {
    onChangeView(view);
    setIsOpen(false);
  }

  return (
    <aside className="border-b border-jarvis-border/70 bg-jarvis-panel/95 backdrop-blur-xl lg:sticky lg:top-0 lg:flex lg:h-screen lg:w-80 lg:flex-col lg:border-b-0 lg:border-r">
      <div className="flex items-center justify-between gap-3 px-4 py-4 lg:px-5">
        <button className="flex min-w-0 items-center gap-3 text-left" type="button" onClick={() => handleNav("chat")}>
          <AssistantOrb size="sm" active={status?.online} />
          <div className="min-w-0">
            <h1 className="truncate text-xl font-black tracking-normal text-jarvis-text">Jarvis</h1>
            <p className="truncate text-sm text-jarvis-muted">
              {status?.online ? "Ollama online" : "Local-first assistant"}
            </p>
          </div>
        </button>
        <button
          type="button"
          className="rounded-xl border border-jarvis-border px-3 py-2 text-sm font-bold text-jarvis-text lg:hidden"
          onClick={() => setIsOpen((current) => !current)}
        >
          Menu
        </button>
      </div>

      <div className={`${isOpen ? "grid" : "hidden"} gap-5 px-4 pb-5 lg:flex lg:min-h-0 lg:flex-1 lg:flex-col lg:px-5 lg:pb-5`}>
        <button
          className="min-h-12 rounded-2xl bg-jarvis-green px-4 text-left text-sm font-black text-jarvis-bg shadow-glow transition hover:-translate-y-0.5"
          type="button"
          onClick={handleNewChat}
        >
          + New Chat
        </button>

        <nav className="grid grid-cols-2 gap-2 lg:grid-cols-1" aria-label="Main navigation">
          {navItems.map((item) => (
            <button
              className={`flex min-h-11 items-center gap-3 rounded-2xl px-3 text-left text-sm font-bold transition ${
                activeView === item.key
                  ? "bg-jarvis-green/15 text-jarvis-green2 ring-1 ring-jarvis-green/30"
                  : "text-jarvis-muted hover:bg-white/[0.04] hover:text-jarvis-text"
              }`}
              key={item.key}
              type="button"
              onClick={() => handleNav(item.key)}
            >
              <span className="grid h-7 w-7 place-items-center rounded-xl bg-white/[0.06] text-xs">{item.symbol}</span>
              {item.label}
            </button>
          ))}
        </nav>

        <section className="min-h-0 lg:flex-1">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-xs font-black uppercase tracking-[0.18em] text-jarvis-faint">Recent Chats</h2>
            <span className="text-xs text-jarvis-faint">{chats.length}</span>
          </div>
          <div className="grid max-h-64 gap-2 overflow-auto pr-1 lg:max-h-none">
            {chats.slice(0, 10).map((chat) => (
              <button
                key={chat.id}
                type="button"
                onClick={() => void handleOpenChat(chat.id)}
                className={`rounded-2xl border p-3 text-left transition hover:border-jarvis-green/50 hover:bg-jarvis-green/10 ${
                  activeChatId === chat.id ? "border-jarvis-green/60 bg-jarvis-green/10" : "border-transparent bg-white/[0.03]"
                }`}
              >
                <strong className="line-clamp-1 text-sm text-jarvis-text">{chat.title}</strong>
                <span className="mt-1 line-clamp-2 block text-xs leading-5 text-jarvis-muted">{chat.preview}</span>
              </button>
            ))}
            {chats.length === 0 && (
              <p className="rounded-2xl border border-dashed border-jarvis-border p-4 text-sm text-jarvis-muted">
                Your saved conversations will appear here.
              </p>
            )}
          </div>
        </section>

        <section className="grid gap-3 rounded-2xl border border-jarvis-border bg-white/[0.035] p-4">
          <div className="flex items-center justify-between">
            <span className="text-sm text-jarvis-muted">Runtime</span>
            <StatusBadge tone={status?.online ? "green" : "amber"}>{status?.online ? "Online" : "Offline"}</StatusBadge>
          </div>
          <div className="flex items-center justify-between gap-3 text-sm">
            <span className="text-jarvis-muted">Model</span>
            <strong className="truncate text-right text-jarvis-text" title={status?.model ?? ""}>{status?.model ?? "-"}</strong>
          </div>
          <div className="flex items-center justify-between text-sm">
            <span className="text-jarvis-muted">Memory</span>
            <strong className="text-jarvis-text">{memoryCount}</strong>
          </div>
        </section>
      </div>
    </aside>
  );
}
