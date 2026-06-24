"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { TopBar } from "./TopBar";
import { MemoryCard } from "./memory/MemoryCard";
import { MemoryEditorForm } from "./memory/MemoryEditorForm";
import { MemoryIngestionPanel } from "./memory/MemoryIngestionPanel";
import { MemorySearchBar } from "./memory/MemorySearchBar";
import {
  cleanMemoryDraft,
  emptyMemoryDraft,
  memoryToDraft,
  type MemoryDraft
} from "./memory/memoryForm";
import { jarvisApi } from "@/lib/api";
import type { MemoryFormValues, MemoryItem } from "@/lib/types";

type MemoryPanelProps = {
  items: MemoryItem[];
  onAdd: (values: MemoryFormValues) => Promise<void>;
  onUpdate: (id: string, values: MemoryFormValues) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  onApprove: (id: string) => Promise<void>;
  onReject: (id: string) => Promise<void>;
  onClear: () => Promise<void>;
  onRefresh: () => Promise<void>;
};

export function MemoryPanel({ items, onAdd, onUpdate, onDelete, onApprove, onReject, onClear, onRefresh }: MemoryPanelProps) {
  const [draft, setDraft] = useState<MemoryFormValues>(emptyMemoryDraft);
  const [displayItems, setDisplayItems] = useState(items);
  const [editing, setEditing] = useState<MemoryDraft | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchCategory, setSearchCategory] = useState("");
  const [searchTag, setSearchTag] = useState("");
  const [searchMemoryType, setSearchMemoryType] = useState("");
  const [searchReviewStatus, setSearchReviewStatus] = useState("");
  const [searchMinImportance, setSearchMinImportance] = useState("");
  const [searchMinConfidence, setSearchMinConfidence] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [isSearching, setIsSearching] = useState(false);
  const [isClearing, setIsClearing] = useState(false);

  const searchActive = useMemo(
    () =>
      Boolean(
        searchQuery.trim() ||
          searchCategory.trim() ||
          searchTag.trim() ||
          searchMemoryType ||
          searchReviewStatus ||
          searchMinImportance ||
          searchMinConfidence
      ),
    [
      searchCategory,
      searchMemoryType,
      searchMinConfidence,
      searchMinImportance,
      searchQuery,
      searchReviewStatus,
      searchTag
    ]
  );

  const sections = useMemo(() => buildMemorySections(displayItems), [displayItems]);

  useEffect(() => {
    if (!searchActive) {
      setDisplayItems(items);
    }
  }, [items, searchActive]);

  async function runSearch(event?: FormEvent<HTMLFormElement>) {
    event?.preventDefault();

    if (!searchActive) {
      setDisplayItems(items);
      return;
    }

    setIsSearching(true);
    try {
      setDisplayItems(
        await jarvisApi.searchMemory({
          query: searchQuery,
          category: searchCategory,
          tag: searchTag,
          minImportance: parseOptionalNumber(searchMinImportance),
          minConfidence: parseOptionalNumber(searchMinConfidence),
          memoryType: searchMemoryType as MemoryItem["memoryType"] | "",
          reviewStatus: searchReviewStatus as MemoryItem["reviewStatus"] | ""
        })
      );
    } finally {
      setIsSearching(false);
    }
  }

  function resetSearch() {
    setSearchQuery("");
    setSearchCategory("");
    setSearchTag("");
    setSearchMemoryType("");
    setSearchReviewStatus("");
    setSearchMinImportance("");
    setSearchMinConfidence("");
    setDisplayItems(items);
  }

  async function handleAdd(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!draft.text.trim()) {
      return;
    }

    await saveMemoryChange(async () => {
      await onAdd(cleanMemoryDraft(draft));
      setDraft(emptyMemoryDraft);
    });
  }

  async function handleUpdate() {
    if (!editing?.id || !editing.text.trim()) {
      return;
    }

    await saveMemoryChange(async () => {
      await onUpdate(editing.id!, cleanMemoryDraft(editing));
      setEditing(null);
    });
  }

  async function handleDelete(id: string) {
    await saveMemoryChange(async () => {
      await onDelete(id);
      if (editing?.id === id) {
        setEditing(null);
      }
    });
  }

  async function handleApprove(id: string) {
    await saveMemoryChange(async () => {
      await onApprove(id);
    });
  }

  async function handleReject(id: string) {
    await saveMemoryChange(async () => {
      await onReject(id);
    });
  }

  async function handleClear() {
    setIsClearing(true);
    try {
      await onClear();
      setSearchQuery("");
      setSearchCategory("");
      setSearchTag("");
      setSearchMemoryType("");
      setSearchReviewStatus("");
      setSearchMinImportance("");
      setSearchMinConfidence("");
      setDisplayItems([]);
    } finally {
      setIsClearing(false);
    }
  }

  async function saveMemoryChange(action: () => Promise<void>) {
    setIsSaving(true);
    try {
      await action();
      if (searchActive) {
        await runSearch();
      }
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="tool-panel">
      <TopBar
        title="Memory"
        subtitle="Review, search, and curate private memories"
        action={
          <button className="danger-button" type="button" onClick={handleClear} disabled={isClearing}>
            {isClearing ? "Clearing" : "Clear"}
          </button>
        }
      />

      <MemorySearchBar
        query={searchQuery}
        category={searchCategory}
        tag={searchTag}
        memoryType={searchMemoryType}
        reviewStatus={searchReviewStatus}
        minImportance={searchMinImportance}
        minConfidence={searchMinConfidence}
        isSearching={isSearching}
        searchActive={searchActive}
        onQueryChange={setSearchQuery}
        onCategoryChange={setSearchCategory}
        onTagChange={setSearchTag}
        onMemoryTypeChange={setSearchMemoryType}
        onReviewStatusChange={setSearchReviewStatus}
        onMinImportanceChange={setSearchMinImportance}
        onMinConfidenceChange={setSearchMinConfidence}
        onSearch={runSearch}
        onReset={resetSearch}
      />

      <MemoryIngestionPanel onMemoryChanged={onRefresh} />

      <MemoryEditorForm
        draft={draft}
        submitLabel="Add memory"
        isSaving={isSaving}
        onChange={setDraft}
        onSubmit={handleAdd}
      />

      <div className="memory-review-summary">
        {sections.map((section) => (
          <span key={section.title}>
            {section.title}: <strong>{section.items.length}</strong>
          </span>
        ))}
      </div>

      <div className="memory-section-list">
        {displayItems.length === 0 && <div className="list-empty">No memories saved yet.</div>}

        {sections.map((section) => (
          <MemorySection
            key={section.title}
            section={section}
            editing={editing}
            isSaving={isSaving}
            onEdit={(memory) => setEditing(memoryToDraft(memory))}
            onCancelEdit={() => setEditing(null)}
            onEditingChange={setEditing}
            onSaveEdit={handleUpdate}
            onDelete={handleDelete}
            onApprove={handleApprove}
            onReject={handleReject}
          />
        ))}
      </div>
    </section>
  );
}

type MemorySectionModel = {
  title: string;
  description: string;
  items: MemoryItem[];
};

type MemorySectionProps = {
  section: MemorySectionModel;
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

function MemorySection({
  section,
  editing,
  isSaving,
  onEdit,
  onCancelEdit,
  onEditingChange,
  onSaveEdit,
  onDelete,
  onApprove,
  onReject
}: MemorySectionProps) {
  return (
    <section className="memory-review-section">
      <div className="memory-section-heading">
        <div>
          <h3>{section.title}</h3>
          <p>{section.description}</p>
        </div>
        <strong>{section.items.length}</strong>
      </div>

      {section.items.length === 0 ? (
        <div className="list-empty compact">No memories in this section.</div>
      ) : (
        <div className="item-list">
          {section.items.map((item) => (
            <MemoryCard
              key={item.id}
              item={item}
              editing={editing}
              isSaving={isSaving}
              onEdit={onEdit}
              onCancelEdit={onCancelEdit}
              onEditingChange={onEditingChange}
              onSaveEdit={onSaveEdit}
              onDelete={onDelete}
              onApprove={onApprove}
              onReject={onReject}
            />
          ))}
        </div>
      )}
    </section>
  );
}

function buildMemorySections(items: MemoryItem[]): MemorySectionModel[] {
  const sorted = [...items].sort((left, right) => new Date(right.updatedAtUtc).getTime() - new Date(left.updatedAtUtc).getTime());
  return [
    {
      title: "Suggested Memories",
      description: "Pending suggestions waiting for approval, rejection, or edit.",
      items: sorted.filter((item) => item.memoryType === "SuggestedMemory" && item.reviewStatus === "Pending")
    },
    {
      title: "Permanent Memories",
      description: "Approved long-term memories available for retrieval.",
      items: sorted.filter(
        (item) =>
          item.memoryType !== "TemporaryContext" &&
          item.reviewStatus !== "Pending" &&
          item.reviewStatus !== "Rejected"
      )
    },
    {
      title: "Temporary Context",
      description: "Short-lived context. Expired items are not used in retrieval.",
      items: sorted.filter((item) => item.memoryType === "TemporaryContext" && item.reviewStatus !== "Rejected")
    },
    {
      title: "Rejected Memories",
      description: "Rejected memories are retained for review and never used in retrieval.",
      items: sorted.filter((item) => item.reviewStatus === "Rejected")
    }
  ];
}

function parseOptionalNumber(value: string) {
  const parsed = Number.parseInt(value, 10);
  return Number.isFinite(parsed) ? parsed : undefined;
}
