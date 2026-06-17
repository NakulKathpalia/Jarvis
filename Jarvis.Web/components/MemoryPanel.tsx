"use client";

import { FormEvent, useState } from "react";
import { TopBar } from "./TopBar";
import type { MemoryItem } from "@/lib/types";

type MemoryPanelProps = {
  items: MemoryItem[];
  onAdd: (text: string) => Promise<void>;
  onClear: () => Promise<void>;
};

export function MemoryPanel({ items, onAdd, onClear }: MemoryPanelProps) {
  const [text, setText] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = text.trim();
    if (!trimmed) {
      return;
    }

    setIsSaving(true);
    await onAdd(trimmed);
    setText("");
    setIsSaving(false);
  }

  return (
    <section className="tool-panel">
      <TopBar
        title="Memory"
        subtitle="Private facts saved to local JSON"
        action={
          <button className="danger-button" type="button" onClick={onClear}>
            Clear
          </button>
        }
      />

      <form className="tool-form" onSubmit={handleSubmit}>
        <input
          value={text}
          placeholder="Remember user preferences, goals, or facts"
          onChange={(event) => setText(event.target.value)}
        />
        <button type="submit" disabled={isSaving || !text.trim()}>
          Add
        </button>
      </form>

      <div className="item-list">
        {items.length === 0 && <div className="list-empty">No memories saved yet.</div>}
        {items.map((item) => (
          <article className="list-card" key={item.id}>
            <strong>{item.text}</strong>
            <span>
              {item.category} · {new Date(item.createdAtUtc).toLocaleString()}
            </span>
          </article>
        ))}
      </div>
    </section>
  );
}
