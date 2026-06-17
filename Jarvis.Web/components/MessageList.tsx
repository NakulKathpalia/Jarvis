import type { ChatMessage } from "@/lib/types";

type MessageListProps = {
  messages: ChatMessage[];
  onSpeak: (text: string) => Promise<void>;
};

export function MessageList({ messages, onSpeak }: MessageListProps) {
  if (messages.length === 0) {
    return (
      <div className="empty-chat">
        <div className="empty-mark">J</div>
        <h2>How can I help?</h2>
        <p>Ask something, save memory, index files, or tune the local model settings.</p>
      </div>
    );
  }

  return (
    <div className="message-list" aria-live="polite">
      {messages.map((message, index) => (
        <article className={`chat-message ${message.role}`} key={`${message.role}-${index}`}>
          <div className="message-avatar">{message.role === "user" ? "You" : "J"}</div>
          <div className="message-body">
            <div className="message-name">{message.role === "user" ? "You" : "Jarvis"}</div>
            <div className="message-text">{message.content}</div>
            {message.role === "assistant" && message.content !== "Thinking..." && (
              <button
                className="speak-message-button"
                type="button"
                onClick={() => void onSpeak(message.content)}
              >
                Speak
              </button>
            )}
          </div>
        </article>
      ))}
    </div>
  );
}
