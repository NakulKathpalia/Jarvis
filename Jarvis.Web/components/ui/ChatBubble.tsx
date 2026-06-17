import type { ChatMessage } from "@/lib/types";

type ChatBubbleProps = {
  message: ChatMessage;
  onSpeak: (text: string) => Promise<void>;
};

export function ChatBubble({ message, onSpeak }: ChatBubbleProps) {
  const isUser = message.role === "user";

  return (
    <article className={`flex w-full ${isUser ? "justify-end" : "justify-start"}`}>
      <div className={`max-w-[82%] rounded-3xl border px-5 py-4 shadow-card ${
        isUser
          ? "border-jarvis-green/30 bg-jarvis-green text-jarvis-bg"
          : "border-jarvis-border bg-jarvis-card text-jarvis-text"
      }`}>
        <div className={`mb-2 text-xs font-bold uppercase tracking-[0.16em] ${isUser ? "text-emerald-950" : "text-jarvis-muted"}`}>
          {isUser ? "You" : "Jarvis"}
        </div>
        <div className="whitespace-pre-wrap break-words text-sm leading-7">{message.content}</div>
        {!isUser && message.content !== "Thinking..." && (
          <button
            className="mt-3 rounded-full border border-jarvis-border px-3 py-1 text-xs font-semibold text-jarvis-muted transition hover:border-jarvis-green/50 hover:text-jarvis-text"
            type="button"
            onClick={() => void onSpeak(message.content)}
          >
            Speak
          </button>
        )}
      </div>
    </article>
  );
}
