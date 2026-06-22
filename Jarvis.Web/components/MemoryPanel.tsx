"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { TopBar } from "./TopBar";
import { MemoryCard } from "./memory/MemoryCard";
import { MemoryEditorForm } from "./memory/MemoryEditorForm";
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
};

export function MemoryPanel({ items, onAdd, onUpdate, onDelete, onApprove, onReject, onClear }: MemoryPanelProps) {
  const [draft, setDraft] = useState<MemoryFormValues>(emptyMemoryDraft);
  const [displayItems, setDisplayItems] = useState(items);
  const [editing, setEditing] = useState<MemoryDraft | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchCategory, setSearchCategory] = useState("");
  const [searchTag, setSearchTag] = useState("");
  const [searchMemoryType, setSearchMemoryType] = useState("");
  const [searchReviewStatus, setSearchReviewStatus] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [isSearching, setIsSearching] = useState(false);
  const [isClearing, setIsClearing] = useState(false);

  const searchActive = useMemo(
    () => Boolean(searchQuery.trim() || searchCategory.trim() || searchTag.trim() || searchMemoryType || searchReviewStatus),
    [searchCategory, searchMemoryType, searchQuery, searchReviewStatus, searchTag]
  );

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
        subtitle="Private facts saved to local JSON"
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
        isSearching={isSearching}
        searchActive={searchActive}
        onQueryChange={setSearchQuery}
        onCategoryChange={setSearchCategory}
        onTagChange={setSearchTag}
        onMemoryTypeChange={setSearchMemoryType}
        onReviewStatusChange={setSearchReviewStatus}
        onSearch={runSearch}
        onReset={resetSearch}
      />

      <MemoryEditorForm
        draft={draft}
        submitLabel="Add memory"
        isSaving={isSaving}
        onChange={setDraft}
        onSubmit={handleAdd}
      />

      <div className="item-list">
        {displayItems.length === 0 && <div className="list-empty">No memories saved yet.</div>}

        {displayItems.map((item) => (
          <MemoryCard
            key={item.id}
            item={item}
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
