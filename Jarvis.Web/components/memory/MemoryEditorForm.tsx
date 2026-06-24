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

const memoryCategories = [
  "Identity",
  "Preferences",
  "Projects",
  "Devices",
  "Education",
  "Work",
  "Astrology",
  "Tarot",
  "Occult",
  "Vastu",
  "Goals",
  "Health",
  "General"
];

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
        <select
          value={draft.category}
          onChange={(event) => onChange({ ...draft, category: event.target.value })}
        >
          {memoryCategories.map((category) => (
            <option key={category} value={category}>
              {category}
            </option>
          ))}
        </select>
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
          max={10}
          value={draft.importance}
          onChange={(event) =>
            onChange({
              ...draft,
              importance: Number(event.target.value) || 3
            })
          }
        />
      </label>

      <label>
        <span>Confidence</span>
        <input
          type="number"
          min={1}
          max={10}
          value={draft.confidence}
          onChange={(event) =>
            onChange({
              ...draft,
              confidence: Number(event.target.value) || 10
            })
          }
        />
      </label>

      <label>
        <span>Memory Type</span>
        <select
          value={draft.memoryType}
          onChange={(event) =>
            onChange({
              ...draft,
              memoryType: event.target.value as MemoryFormValues["memoryType"],
              reviewStatus: event.target.value === "SuggestedMemory" ? "Pending" : "Approved"
            })
          }
        >
          <option value="TemporaryContext">Temporary Context</option>
          <option value="SuggestedMemory">Suggested Memory</option>
          <option value="PermanentMemory">Permanent Memory</option>
        </select>
      </label>

      <label>
        <span>Review Status</span>
        <select
          value={draft.reviewStatus ?? "Approved"}
          onChange={(event) =>
            onChange({
              ...draft,
              reviewStatus: event.target.value as MemoryFormValues["reviewStatus"]
            })
          }
        >
          <option value="Pending">Pending</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
      </label>

      <label>
        <span>Source</span>
        <input
          value={draft.source}
          placeholder="Manual, Chat, Voice"
          onChange={(event) => onChange({ ...draft, source: event.target.value })}
        />
      </label>

      {draft.memoryType === "TemporaryContext" && (
        <label>
          <span>Expires At</span>
          <input
            type="datetime-local"
            value={toDateTimeLocal(draft.expiresAtUtc)}
            onChange={(event) =>
              onChange({
                ...draft,
                expiresAtUtc: event.target.value ? new Date(event.target.value).toISOString() : null
              })
            }
          />
        </label>
      )}

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

function toDateTimeLocal(value?: string | null) {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  const offset = date.getTimezoneOffset();
  const localDate = new Date(date.getTime() - offset * 60_000);
  return localDate.toISOString().slice(0, 16);
}
