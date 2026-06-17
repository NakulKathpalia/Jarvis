import type { ChatSessionSummary } from "@/lib/types";

type ChatCardProps = {
  chat: ChatSessionSummary;
  active?: boolean;
  onOpen: () => void;
  onDelete: () => void;
};

export function ChatCard({ chat, active = false, onOpen, onDelete }: ChatCardProps) {
  return (
    <article className={`group relative overflow-hidden rounded-2xl border bg-jarvis-card p-5 shadow-card transition hover:-translate-y-1 hover:border-jarvis-green/60 ${
      active ? "border-jarvis-green/70" : "border-jarvis-border"
    }`}>
      <button className="absolute inset-0 z-0" type="button" aria-label={`Open ${chat.title}`} onClick={onOpen} />
      <div className="pointer-events-none absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-jarvis-green to-jarvis-green2 opacity-80" />
      <div className="relative z-10 grid gap-4">
        <div>
          <h3 className="line-clamp-2 text-base font-bold text-jarvis-text">{chat.title}</h3>
          <p className="mt-2 line-clamp-3 text-sm leading-6 text-jarvis-muted">{chat.preview}</p>
        </div>
        <div className="flex items-center justify-between gap-3 text-xs text-jarvis-faint">
          <span>{formatDate(chat.updatedAtUtc)}</span>
          <span>{chat.messageCount} messages</span>
        </div>
        <button
          className="relative z-20 justify-self-start rounded-full border border-jarvis-border px-3 py-1 text-xs font-semibold text-jarvis-muted opacity-0 transition hover:border-jarvis-danger/60 hover:text-rose-200 group-hover:opacity-100"
          type="button"
          onClick={onDelete}
        >
          Delete
        </button>
      </div>
    </article>
  );
}

function formatDate(value: string) {
  return new Date(value).toLocaleString([], {
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit"
  });
}
