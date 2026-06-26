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
import type { KnowledgeCategory, KnowledgeItem, KnowledgeStats, MemoryFormValues, MemoryItem, MemoryStats } from "@/lib/types";

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
  const [activeTab, setActiveTab] = useState<"memory" | "knowledge">("memory");
  const [draft, setDraft] = useState<MemoryFormValues>(emptyMemoryDraft);
  const [displayItems, setDisplayItems] = useState(items);
  const [editing, setEditing] = useState<MemoryDraft | null>(null);
  const [selectedMemoryIds, setSelectedMemoryIds] = useState<string[]>([]);
  const [expandedMemoryIds, setExpandedMemoryIds] = useState<string[]>([]);
  const [collapsedSections, setCollapsedSections] = useState<string[]>([]);
  const [visibleCounts, setVisibleCounts] = useState<Record<string, number>>({});
  const [bulkCategory, setBulkCategory] = useState("General");
  const [bulkImportance, setBulkImportance] = useState(3);
  const [bulkConfidence, setBulkConfidence] = useState(7);
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
  const [memoryStats, setMemoryStats] = useState<MemoryStats | null>(null);
  const [knowledgeStats, setKnowledgeStats] = useState<KnowledgeStats | null>(null);
  const [knowledgeItems, setKnowledgeItems] = useState<KnowledgeItem[]>([]);
  const [knowledgeQuery, setKnowledgeQuery] = useState("");
  const [knowledgeCategory, setKnowledgeCategory] = useState<KnowledgeCategory | "">("");
  const [knowledgeSource, setKnowledgeSource] = useState("");
  const [knowledgeFrom, setKnowledgeFrom] = useState("");
  const [knowledgeTo, setKnowledgeTo] = useState("");
  const [statusMessage, setStatusMessage] = useState("");

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

  useEffect(() => {
    refreshStatsAndKnowledge().catch(() => undefined);
  }, [items.length]);

  async function refreshStatsAndKnowledge() {
    const [stats, knowledge, knowledgeSummary] = await Promise.all([
      jarvisApi.memoryStats(),
      jarvisApi.knowledge(),
      jarvisApi.knowledgeStats()
    ]);
    setMemoryStats(stats);
    setKnowledgeItems(knowledge);
    setKnowledgeStats(knowledgeSummary);
  }

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

  async function runKnowledgeSearch() {
    setIsSearching(true);
    try {
      setKnowledgeItems(await jarvisApi.searchKnowledge({
        query: knowledgeQuery,
        category: knowledgeCategory,
        sourceType: knowledgeSource,
        importedAfterUtc: knowledgeFrom ? new Date(`${knowledgeFrom}T00:00:00`).toISOString() : undefined,
        importedBeforeUtc: knowledgeTo ? new Date(`${knowledgeTo}T23:59:59`).toISOString() : undefined,
        limit: 80
      }));
      setKnowledgeStats(await jarvisApi.knowledgeStats());
    } finally {
      setIsSearching(false);
    }
  }

  async function resetKnowledgeSearch() {
    setKnowledgeQuery("");
    setKnowledgeCategory("");
    setKnowledgeSource("");
    setKnowledgeFrom("");
    setKnowledgeTo("");
    await refreshStatsAndKnowledge();
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
      await onRefresh();
      await refreshStatsAndKnowledge();
      if (searchActive) {
        await runSearch();
      }
    } finally {
      setIsSaving(false);
    }
  }

  async function handleBulkAction(action: "approve" | "reject" | "delete" | "update" | "convert") {
    if (selectedMemoryIds.length === 0) {
      setStatusMessage("Select memories first.");
      return;
    }

    setIsSaving(true);
    try {
      const result =
        action === "approve" ? await jarvisApi.bulkApproveMemory(selectedMemoryIds) :
        action === "reject" ? await jarvisApi.bulkRejectMemory(selectedMemoryIds) :
        action === "delete" ? await jarvisApi.bulkDeleteMemory(selectedMemoryIds) :
        await jarvisApi.bulkUpdateMemory(selectedMemoryIds, {
          category: bulkCategory,
          importance: bulkImportance,
          confidence: bulkConfidence,
          convertSuggestedToPermanent: action === "convert"
        });
      setDisplayItems(result.memories);
      setSelectedMemoryIds([]);
      await onRefresh();
      await refreshStatsAndKnowledge();
      setStatusMessage("Bulk action completed.");
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="tool-panel">
      <TopBar
        title="Memory"
        subtitle="Review personal memories and separate reference knowledge"
        action={
          <button className="danger-button" type="button" onClick={handleClear} disabled={isClearing}>
            {isClearing ? "Clearing" : "Clear"}
          </button>
        }
      />

      <div className="memory-tabs">
        <button className={activeTab === "memory" ? "active" : ""} type="button" onClick={() => setActiveTab("memory")}>
          Memory
        </button>
        <button className={activeTab === "knowledge" ? "active" : ""} type="button" onClick={() => setActiveTab("knowledge")}>
          Knowledge
        </button>
      </div>

      {activeTab === "memory" && (
        <>
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

      <MemoryStatsPanel stats={memoryStats} />

      <BulkMemoryToolbar
        selectedCount={selectedMemoryIds.length}
        category={bulkCategory}
        importance={bulkImportance}
        confidence={bulkConfidence}
        disabled={isSaving}
        onCategoryChange={setBulkCategory}
        onImportanceChange={setBulkImportance}
        onConfidenceChange={setBulkConfidence}
        onApprove={() => void handleBulkAction("approve")}
        onReject={() => void handleBulkAction("reject")}
        onDelete={() => void handleBulkAction("delete")}
        onUpdate={() => void handleBulkAction("update")}
        onConvert={() => void handleBulkAction("convert")}
      />
      {statusMessage && <div className="control-message">{statusMessage}</div>}

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
            collapsed={collapsedSections.includes(section.title)}
            visibleCount={visibleCounts[section.title] ?? 20}
            selectedIds={selectedMemoryIds}
            expandedIds={expandedMemoryIds}
            editing={editing}
            isSaving={isSaving}
            onToggleCollapsed={() => setCollapsedSections((current) => toggleValue(current, section.title))}
            onLoadMore={() => setVisibleCounts((current) => ({ ...current, [section.title]: (current[section.title] ?? 20) + 20 }))}
            onSelectAll={() => setSelectedMemoryIds((current) => mergeIds(current, section.items.map((item) => item.id)))}
            onToggleSelected={(id) => setSelectedMemoryIds((current) => toggleValue(current, id))}
            onToggleExpanded={(id) => setExpandedMemoryIds((current) => toggleValue(current, id))}
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
        </>
      )}

      {activeTab === "knowledge" && (
        <KnowledgePanel
          items={knowledgeItems}
          stats={knowledgeStats}
          query={knowledgeQuery}
          category={knowledgeCategory}
          source={knowledgeSource}
          from={knowledgeFrom}
          to={knowledgeTo}
          isSearching={isSearching}
          onQueryChange={setKnowledgeQuery}
          onCategoryChange={setKnowledgeCategory}
          onSourceChange={setKnowledgeSource}
          onFromChange={setKnowledgeFrom}
          onToChange={setKnowledgeTo}
          onSearch={runKnowledgeSearch}
          onReset={resetKnowledgeSearch}
        />
      )}
    </section>
  );
}

function MemoryStatsPanel({ stats }: { stats: MemoryStats | null }) {
  if (!stats) {
    return null;
  }

  return (
    <section className="simple-card memory-stats-panel">
      <div className="memory-section-heading compact">
        <div>
          <h3>Memory Statistics</h3>
          <p>Quick health check for the personal memory store.</p>
        </div>
      </div>
      <div className="memory-review-summary">
        <span>Total: <strong>{stats.total}</strong></span>
        <span>Approved: <strong>{stats.approved}</strong></span>
        <span>Pending: <strong>{stats.pending}</strong></span>
        <span>Rejected: <strong>{stats.rejected}</strong></span>
      </div>
      <div className="memory-review-summary">
        {stats.categories.slice(0, 8).map((item) => (
          <span key={item.category}>{item.category}: <strong>{item.count}</strong></span>
        ))}
      </div>
    </section>
  );
}

function BulkMemoryToolbar({
  selectedCount,
  category,
  importance,
  confidence,
  disabled,
  onCategoryChange,
  onImportanceChange,
  onConfidenceChange,
  onApprove,
  onReject,
  onDelete,
  onUpdate,
  onConvert
}: {
  selectedCount: number;
  category: string;
  importance: number;
  confidence: number;
  disabled: boolean;
  onCategoryChange: (value: string) => void;
  onImportanceChange: (value: number) => void;
  onConfidenceChange: (value: number) => void;
  onApprove: () => void;
  onReject: () => void;
  onDelete: () => void;
  onUpdate: () => void;
  onConvert: () => void;
}) {
  return (
    <section className="simple-card bulk-toolbar">
      <div className="memory-section-heading compact">
        <div>
          <h3>Bulk Management</h3>
          <p>{selectedCount} selected. Apply fields or review actions without opening each card.</p>
        </div>
      </div>
      <div className="memory-form compact">
        <label>
          <span>Category</span>
          <select value={category} onChange={(event) => onCategoryChange(event.target.value)}>
            {["Astrology", "Tarot", "Occult", "Vastu", "Projects", "Preferences", "Identity", "Education", "Work", "Goals", "Health", "General"].map((item) => (
              <option key={item} value={item}>{item}</option>
            ))}
          </select>
        </label>
        <label>
          <span>Importance</span>
          <input type="number" min={1} max={10} value={importance} onChange={(event) => onImportanceChange(Number(event.target.value) || 3)} />
        </label>
        <label>
          <span>Confidence</span>
          <input type="number" min={1} max={10} value={confidence} onChange={(event) => onConfidenceChange(Number(event.target.value) || 7)} />
        </label>
      </div>
      <div className="memory-actions">
        <button className="soft-button" type="button" disabled={disabled || selectedCount === 0} onClick={onUpdate}>Apply Fields</button>
        <button className="soft-button" type="button" disabled={disabled || selectedCount === 0} onClick={onConvert}>Convert to Permanent</button>
        <button className="soft-button" type="button" disabled={disabled || selectedCount === 0} onClick={onApprove}>Approve</button>
        <button className="danger-button" type="button" disabled={disabled || selectedCount === 0} onClick={onReject}>Reject</button>
        <button className="danger-button" type="button" disabled={disabled || selectedCount === 0} onClick={onDelete}>Delete</button>
      </div>
    </section>
  );
}

function KnowledgePanel({
  items,
  stats,
  query,
  category,
  source,
  from,
  to,
  isSearching,
  onQueryChange,
  onCategoryChange,
  onSourceChange,
  onFromChange,
  onToChange,
  onSearch,
  onReset
}: {
  items: KnowledgeItem[];
  stats: KnowledgeStats | null;
  query: string;
  category: KnowledgeCategory | "";
  source: string;
  from: string;
  to: string;
  isSearching: boolean;
  onQueryChange: (value: string) => void;
  onCategoryChange: (value: KnowledgeCategory | "") => void;
  onSourceChange: (value: string) => void;
  onFromChange: (value: string) => void;
  onToChange: (value: string) => void;
  onSearch: () => Promise<void>;
  onReset: () => Promise<void>;
}) {
  const terms = query.split(" ", 5).filter(Boolean);
  return (
    <section className="simple-card knowledge-panel">
      <div className="memory-section-heading compact">
        <div>
          <h3>Knowledge</h3>
          <p>Reference material from OCR and PDFs. Separate from personal memories.</p>
        </div>
        <strong>{stats?.total ?? items.length}</strong>
      </div>
      <div className="memory-form compact">
        <label>
          <span>Search Knowledge</span>
          <input value={query} placeholder="Search reference text" onChange={(event) => onQueryChange(event.target.value)} />
        </label>
        <label>
          <span>Category</span>
          <select value={category} onChange={(event) => onCategoryChange(event.target.value as KnowledgeCategory | "")}>
            <option value="">All</option>
            {["Astrology", "Tarot", "Occult", "Vastu", "Research", "Books", "Documents", "General"].map((item) => (
              <option key={item} value={item}>{item}</option>
            ))}
          </select>
        </label>
        <label>
          <span>Source</span>
          <select value={source} onChange={(event) => onSourceChange(event.target.value)}>
            <option value="">All</option>
            <option value="Image">OCR/Image</option>
            <option value="Pdf">PDF</option>
            <option value="Manual">Manual</option>
          </select>
        </label>
        <label>
          <span>From</span>
          <input type="date" value={from} onChange={(event) => onFromChange(event.target.value)} />
        </label>
        <label>
          <span>To</span>
          <input type="date" value={to} onChange={(event) => onToChange(event.target.value)} />
        </label>
      </div>
      <div className="memory-actions">
        <button className="soft-button" type="button" disabled={isSearching} onClick={() => void onSearch()}>{isSearching ? "Searching" : "Search"}</button>
        <button className="soft-button" type="button" disabled={isSearching} onClick={() => void onReset()}>Reset</button>
      </div>
      {stats && (
        <div className="memory-review-summary">
          {stats.categories.slice(0, 6).map((item) => <span key={item.category}>{item.category}: <strong>{item.count}</strong></span>)}
          {stats.sources.slice(0, 4).map((item) => <span key={item.sourceType}>{item.sourceType}: <strong>{item.count}</strong></span>)}
        </div>
      )}
      <div className="item-list compact">
        {items.slice(0, 40).map((item) => (
          <article className="list-card memory-card" key={item.id}>
            <div className="memory-card-head">
              <strong>{highlight(item.title, terms)}</strong>
              <div className="memory-badges">
                <span className="memory-badge">{item.category}</span>
                <span className="memory-badge">{item.sourceType || "Manual"}</span>
                <span className="memory-badge">{item.wordCount} words</span>
                <span className="memory-badge">{item.accessCount} views</span>
              </div>
            </div>
            <p>{highlight(item.content.slice(0, 420), terms)}{item.content.length > 420 ? "..." : ""}</p>
          </article>
        ))}
        {items.length === 0 && <div className="list-empty compact">No knowledge saved yet.</div>}
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
  collapsed: boolean;
  visibleCount: number;
  selectedIds: string[];
  expandedIds: string[];
  editing: MemoryDraft | null;
  isSaving: boolean;
  onToggleCollapsed: () => void;
  onLoadMore: () => void;
  onSelectAll: () => void;
  onToggleSelected: (id: string) => void;
  onToggleExpanded: (id: string) => void;
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
  collapsed,
  visibleCount,
  selectedIds,
  expandedIds,
  editing,
  isSaving,
  onToggleCollapsed,
  onLoadMore,
  onSelectAll,
  onToggleSelected,
  onToggleExpanded,
  onEdit,
  onCancelEdit,
  onEditingChange,
  onSaveEdit,
  onDelete,
  onApprove,
  onReject
}: MemorySectionProps) {
  const visibleItems = section.items.slice(0, visibleCount);

  return (
    <section className="memory-review-section">
      <div className="memory-section-heading">
        <button className="section-toggle" type="button" onClick={onToggleCollapsed}>
          {collapsed ? "+" : "-"}
        </button>
        <div>
          <h3>{section.title}</h3>
          <p>{section.description}</p>
        </div>
        <strong>{section.items.length}</strong>
      </div>

      {!collapsed && (
        <>
          <div className="memory-actions">
            <button className="soft-button" type="button" disabled={section.items.length === 0} onClick={onSelectAll}>
              Select All
            </button>
          </div>
          {visibleItems.length === 0 ? (
            <div className="list-empty compact">No memories in this section.</div>
          ) : (
            <div className="item-list compact">
              {visibleItems.map((item) => (
                <CompactMemoryRow
                  key={item.id}
                  item={item}
                  selected={selectedIds.includes(item.id)}
                  expanded={expandedIds.includes(item.id)}
                  editing={editing}
                  isSaving={isSaving}
                  onToggleSelected={() => onToggleSelected(item.id)}
                  onToggleExpanded={() => onToggleExpanded(item.id)}
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
          {section.items.length > visibleItems.length && (
            <button className="soft-button load-more-button" type="button" onClick={onLoadMore}>
              Load 20 more
            </button>
          )}
        </>
      )}
    </section>
  );
}

function CompactMemoryRow({
  item,
  selected,
  expanded,
  editing,
  isSaving,
  onToggleSelected,
  onToggleExpanded,
  onEdit,
  onCancelEdit,
  onEditingChange,
  onSaveEdit,
  onDelete,
  onApprove,
  onReject
}: {
  item: MemoryItem;
  selected: boolean;
  expanded: boolean;
  editing: MemoryDraft | null;
  isSaving: boolean;
  onToggleSelected: () => void;
  onToggleExpanded: () => void;
  onEdit: (item: MemoryItem) => void;
  onCancelEdit: () => void;
  onEditingChange: (draft: MemoryDraft) => void;
  onSaveEdit: () => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  onApprove: (id: string) => Promise<void>;
  onReject: (id: string) => Promise<void>;
}) {
  return (
    <article className="memory-row-card">
      <div className="memory-row-main">
        <input type="checkbox" checked={selected} onChange={onToggleSelected} />
        <button className="memory-row-text" type="button" onClick={onToggleExpanded}>
          <strong>{item.text.slice(0, 120)}{item.text.length > 120 ? "..." : ""}</strong>
          <span>{item.category} - {item.memoryType} - {item.reviewStatus} - I{item.importance} C{item.confidence}</span>
        </button>
        <button className="soft-button" type="button" onClick={onToggleExpanded}>{expanded ? "Collapse" : "Expand"}</button>
      </div>
      {expanded && (
        <MemoryCard
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
      )}
    </article>
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

function toggleValue(values: string[], value: string) {
  return values.includes(value) ? values.filter((item) => item !== value) : [...values, value];
}

function mergeIds(current: string[], ids: string[]) {
  return Array.from(new Set([...current, ...ids]));
}

function highlight(text: string, terms: string[]) {
  if (terms.length === 0) {
    return text;
  }

  let result = text;
  for (const term of terms) {
    result = result.replace(new RegExp(`(${escapeRegExp(term)})`, "ig"), "[$1]");
  }

  return result;
}

function escapeRegExp(value: string) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}
