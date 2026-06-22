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
  onApprove: (id: string) => Promise<void>;
  onReject: (id: string) => Promise<void>;
};

export function MemoryCard({
  item,
  editing,
  isSaving,
  onEdit,
  onCancelEdit,
  onEditingChange,
  onSaveEdit,
  onDelete,
  onApprove,
  onReject
}: MemoryCardProps) {
  const expired = isExpired(item);
  const qualityIndicators = getQualityIndicators(item, expired);

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
    <article className={expired ? "list-card memory-card expired" : "list-card memory-card"}>
      <div className="memory-card-head">
        <strong>{item.text}</strong>
        <div className="memory-badges">
          <span className="memory-badge">P{item.importance}</span>
          <span className="memory-badge">C{item.confidence ?? 10}</span>
          <span className="memory-badge">{formatMemoryType(item.memoryType)}</span>
          <span className="memory-badge">{item.reviewStatus ?? "Approved"}</span>
          <span className="memory-badge">{item.category}</span>
        </div>
      </div>

      {qualityIndicators.length > 0 && (
        <div className="memory-quality-list">
          {qualityIndicators.map((indicator) => (
            <span className={`memory-quality ${indicator.tone}`} key={`${item.id}-${indicator.label}`}>
              {indicator.label}
            </span>
          ))}
        </div>
      )}

      {item.tags.length > 0 && (
        <div className="memory-tags">
          {item.tags.map((tag) => (
            <span className="memory-tag" key={`${item.id}-${tag}`}>
              #{tag}
            </span>
          ))}
        </div>
      )}

      <dl className="memory-meta-grid">
        <div>
          <dt>Category</dt>
          <dd>{item.category}</dd>
        </div>
        <div>
          <dt>Type</dt>
          <dd>{formatMemoryType(item.memoryType)}</dd>
        </div>
        <div>
          <dt>Status</dt>
          <dd>{item.reviewStatus ?? "Approved"}</dd>
        </div>
        <div>
          <dt>Importance</dt>
          <dd>{item.importance}</dd>
        </div>
        <div>
          <dt>Confidence</dt>
          <dd>{item.confidence ?? 10}</dd>
        </div>
        <div>
          <dt>Source</dt>
          <dd>{item.source || "Manual"}</dd>
        </div>
        <div>
          <dt>Created</dt>
          <dd>{formatDate(item.createdAtUtc)}</dd>
        </div>
        <div>
          <dt>Updated</dt>
          <dd>{formatDate(item.updatedAtUtc)}</dd>
        </div>
        {item.memoryType === "TemporaryContext" && (
          <div>
            <dt>Expires</dt>
            <dd>{item.expiresAtUtc ? formatDate(item.expiresAtUtc) : "No expiry set"}</dd>
          </div>
        )}
      </dl>

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
        {item.reviewStatus !== "Approved" && (
          <button type="button" className="soft-button" onClick={() => onApprove(item.id)} disabled={isSaving}>
            Approve
          </button>
        )}
        {item.reviewStatus !== "Rejected" && (
          <button type="button" className="soft-button" onClick={() => onReject(item.id)} disabled={isSaving}>
            Reject
          </button>
        )}
      </div>
    </article>
  );
}

function isExpired(item: MemoryItem) {
  return Boolean(
    item.memoryType === "TemporaryContext" &&
      item.expiresAtUtc &&
      new Date(item.expiresAtUtc).getTime() <= Date.now()
  );
}

function getQualityIndicators(item: MemoryItem, expired: boolean) {
  const indicators: Array<{ label: string; tone: "good" | "warn" | "danger" | "neutral" }> = [];

  if ((item.confidence ?? 10) >= 8) {
    indicators.push({ label: "High confidence", tone: "good" });
  }

  if ((item.confidence ?? 10) <= 4) {
    indicators.push({ label: "Low confidence", tone: "warn" });
  }

  if (item.importance >= 8) {
    indicators.push({ label: "High importance", tone: "good" });
  }

  if (expired) {
    indicators.push({ label: "Expired", tone: "danger" });
  }

  if (item.reviewStatus === "Pending") {
    indicators.push({ label: "Pending review", tone: "warn" });
  }

  if (item.reviewStatus === "Rejected") {
    indicators.push({ label: "Rejected", tone: "danger" });
  }

  return indicators;
}

function formatDate(value: string) {
  return new Date(value).toLocaleString();
}

function formatMemoryType(value?: string) {
  switch (value) {
    case "TemporaryContext":
      return "Temporary";
    case "SuggestedMemory":
      return "Suggested";
    case "PermanentMemory":
      return "Permanent";
    default:
      return "Permanent";
  }
}
