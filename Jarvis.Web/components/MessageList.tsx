import { AssistantOrb } from "./ui/AssistantOrb";
import { ChatBubble } from "./ui/ChatBubble";
import type { ChatMessage } from "@/lib/types";

type MessageListProps = {
  messages: ChatMessage[];
  isBusy?: boolean;
  onSpeak: (text: string) => Promise<void>;
};

export function MessageList({ messages, isBusy = false, onSpeak }: MessageListProps) {
  if (messages.length === 0) {
    return (
      <div className="grid place-items-center px-5 py-10 text-center">
        <div className="grid max-w-xl place-items-center gap-5">
          <AssistantOrb size="lg" active={isBusy} />
          <div>
            <h2 className="text-3xl font-black text-jarvis-text sm:text-4xl">How can I help?</h2>
            <p className="mt-3 text-sm leading-7 text-jarvis-muted sm:text-base">
              Ask Jarvis to search files, remember details, control your PC safely, or talk through Ollama locally.
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex w-full max-w-4xl flex-1 flex-col gap-5 px-4 py-6" aria-live="polite">
      {messages.map((message, index) => (
        <ChatBubble message={message} onSpeak={onSpeak} key={`${message.role}-${index}-${message.createdAtUtc ?? ""}`} />
      ))}
    </div>
  );
}
