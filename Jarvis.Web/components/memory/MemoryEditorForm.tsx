"use client";

import { FormEvent } from "react";
import type { MemoryFormValues } from "@/lib/types";
import { parseMemoryTags } from "./memoryForm";

type MemoryEditorFormProps = {
  draft: MemoryFormValues;
  submitLabel: string;
  isSaving: boolean;
  className?: string;
  onChange: (draft: MemoryFormValues) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
  onCancel?: () => void;
};

export function MemoryEditorForm({
  draft,
  submitLabel,
  isSaving,
  className = "memory-form",
  onChange,
  onSubmit,
  onCancel
}: MemoryEditorFormProps) {
  return (
    <form className={className} onSubmit={onSubmit}>
      <label>
        <span>Memory text</span>
        <textarea
          rows={3}
          value={draft.text}
          placeholder="Remember user preferences, goals, or facts"
          onChange={(event) => onChange({ ...draft, text: event.target.value })}
        />
      </label>

      <label>
        <span>Category</span>
        <input
          value={draft.category}
          placeholder="General"
          onChange={(event) => onChange({ ...draft, category: event.target.value })}
        />
      </label>

      <label>
        <span>Tags</span>
        <input
          value={draft.tags.join(", ")}
          placeholder="personal, goals"
          onChange={(event) => onChange({ ...draft, tags: parseMemoryTags(event.target.value) })}
        />
      </label>

      <label>
        <span>Importance</span>
        <input
          type="number"
          min={1}
          max={5}
          value={draft.importance}
          onChange={(event) =>
            onChange({
              ...draft,
              importance: Number(event.target.value) || 3
            })
          }
        />
      </label>

      <div className="memory-actions">
        <button type="submit" disabled={isSaving || !draft.text.trim()}>
          {isSaving ? "Saving" : submitLabel}
        </button>
        {onCancel && (
          <button type="button" className="soft-button" onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}
