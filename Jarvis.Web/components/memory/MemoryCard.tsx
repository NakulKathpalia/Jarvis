"use client";

import type { MemoryFormValues, MemoryItem } from "@/lib/types";
import { MemoryEditorForm } from "./MemoryEditorForm";
import type { MemoryDraft } from "./memoryForm";

type MemoryCardProps = {
  item: MemoryItem;
  editing: MemoryDraft | null;
  isSaving: boolean;
  onEdit: (item: MemoryItem) => void;
  onCancelEdit: () => void;
  onEditingChange: (draft: MemoryDraft) => void;
  onSaveEdit: () => Promise<void>;
  onDelete: (id: string) => Promise<void>;
};

export function MemoryCard({
  item,
  editing,
  isSaving,
  onEdit,
  onCancelEdit,
  onEditingChange,
  onSaveEdit,
  onDelete
}: MemoryCardProps) {
  if (editing?.id === item.id) {
    return (
      <MemoryEditorForm
        className="list-card memory-card editing"
        draft={editing}
        submitLabel="Save"
        isSaving={isSaving}
        onChange={(draft: MemoryFormValues) => onEditingChange({ ...draft, id: item.id })}
        onSubmit={async (event) => {
          event.preventDefault();
          await onSaveEdit();
        }}
        onCancel={onCancelEdit}
      />
    );
  }

  return (
    <article className="list-card memory-card">
      <div className="memory-card-head">
        <strong>{item.text}</strong>
        <div className="memory-badges">
          <span className="memory-badge">P{item.importance}</span>
          <span className="memory-badge">{item.category}</span>
        </div>
      </div>

      {item.tags.length > 0 && (
        <div className="memory-tags">
          {item.tags.map((tag) => (
            <span className="memory-tag" key={`${item.id}-${tag}`}>
              #{tag}
            </span>
          ))}
        </div>
      )}

      <span>
        Created {new Date(item.createdAtUtc).toLocaleString()} - Updated{" "}
        {new Date(item.updatedAtUtc).toLocaleString()}
      </span>

      <div className="memory-actions">
        <button type="button" className="soft-button" onClick={() => onEdit(item)}>
          Edit
        </button>
        <button
          type="button"
          className="danger-button"
          onClick={() => onDelete(item.id)}
          disabled={isSaving}
        >
          Delete
        </button>
      </div>
    </article>
  );
}
