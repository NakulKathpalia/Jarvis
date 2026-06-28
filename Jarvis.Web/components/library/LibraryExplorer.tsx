"use client";

import { useMemo, useState } from "react";
import type { IngestionJob, KnowledgeItem, MemoryItem } from "@/lib/types";

type LibraryExplorerProps = {
  memories: MemoryItem[];
  knowledgeItems: KnowledgeItem[];
  ingestionJobs: IngestionJob[];
};

type LibraryLocation =
  | "quickAccess"
  | "memoryAll"
  | "memorySuggested"
  | "memoryPermanent"
  | "memoryTemporary"
  | "memoryRejected"
  | "knowledgeAll"
  | "knowledgeAstrology"
  | "knowledgeVastu"
  | "knowledgeOccult"
  | "knowledgeBooks"
  | "knowledgeDocuments"
  | "importsAll"
  | "importsPending"
  | "importsImages"
  | "importsPdfs";

type LibraryItem = {
  id: string;
  rawId: string;
  name: string;
  type: "Memory" | "Knowledge" | "Import";
  category: string;
  status: string;
  source: string;
  modified: string;
  content: string;
};

const locations: Record<LibraryLocation, string> = {
  quickAccess: "Quick Access",
  memoryAll: "Memory",
  memorySuggested: "Suggested Memories",
  memoryPermanent: "Permanent Memories",
  memoryTemporary: "Temporary Context",
  memoryRejected: "Rejected Memories",
  knowledgeAll: "Knowledge",
  knowledgeAstrology: "Astrology",
  knowledgeVastu: "Vastu",
  knowledgeOccult: "Occult",
  knowledgeBooks: "Books",
  knowledgeDocuments: "Documents",
  importsAll: "Imports",
  importsPending: "Pending Review",
  importsImages: "Image Imports",
  importsPdfs: "PDF Imports"
};

export function LibraryExplorer({ memories, knowledgeItems, ingestionJobs }: LibraryExplorerProps) {
  const [location, setLocation] = useState<LibraryLocation>("quickAccess");
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [query, setQuery] = useState("");

  const allRows = useMemo<LibraryItem[]>(() => {
    const memoryRows = memories.map((item) => ({
      id: `memory-${item.id}`,
      rawId: item.id,
      name: titleFromText(item.text, "Untitled memory"),
      type: "Memory" as const,
      category: item.category || "General",
      status: item.reviewStatus || "Approved",
      source: item.source || "Manual",
      modified: item.updatedAtUtc || item.createdAtUtc,
      content: item.text || ""
    }));

    const knowledgeRows = knowledgeItems.map((item) => ({
      id: `knowledge-${item.id}`,
      rawId: item.id,
      name: item.title || titleFromText(item.content, "Untitled knowledge"),
      type: "Knowledge" as const,
      category: item.category || "General",
      status: "Saved",
      source: item.sourceType || "Manual",
      modified: item.updatedAtUtc || item.createdAtUtc,
      content: item.content || ""
    }));

    const importRows = ingestionJobs.map((item) => ({
      id: `import-${item.id}`,
      rawId: item.id,
      name: item.originalFileName || "Import job",
      type: "Import" as const,
      category: item.sourceType || "Import",
      status: item.status || "Uploaded",
      source: item.sourceType || "Upload",
      modified: item.updatedAtUtc || item.createdAtUtc,
      content: item.extractedText || item.errorMessage || "No extracted text yet."
    }));

    return [...memoryRows, ...knowledgeRows, ...importRows].sort((a, b) =>
      (b.modified || "").localeCompare(a.modified || "")
    );
  }, [memories, knowledgeItems, ingestionJobs]);

  const locationRows = useMemo(() => {
    return allRows.filter((item) => matchesLocation(item, location));
  }, [allRows, location]);

  const filteredRows = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase();

    if (!normalizedQuery) {
      return locationRows;
    }

    return locationRows.filter((item) => {
      const text = `${item.name} ${item.type} ${item.category} ${item.status} ${item.source} ${item.content}`.toLowerCase();
      return text.includes(normalizedQuery);
    });
  }, [locationRows, query]);

  const selected = filteredRows.find((item) => item.id === selectedId) ?? filteredRows[0] ?? null;

  return (
    <section className="library-explorer">
      <aside className="library-nav">
        <h3>Library</h3>

        <NavButton
          active={location === "quickAccess"}
          label="Quick Access"
          onClick={() => setLocation("quickAccess")}
        />

        <div className="library-nav-group">
          <strong>Memory</strong>
          <NavButton active={location === "memoryAll"} label={`All Memories (${count(allRows, "Memory")})`} onClick={() => setLocation("memoryAll")} />
          <NavButton active={location === "memorySuggested"} label={`Suggested (${countMemoryStatus(allRows, "Suggested")})`} onClick={() => setLocation("memorySuggested")} />
          <NavButton active={location === "memoryPermanent"} label={`Permanent (${countMemoryStatus(allRows, "Approved")})`} onClick={() => setLocation("memoryPermanent")} />
          <NavButton active={location === "memoryTemporary"} label={`Temporary (${countMemoryStatus(allRows, "Temporary")})`} onClick={() => setLocation("memoryTemporary")} />
          <NavButton active={location === "memoryRejected"} label={`Rejected (${countMemoryStatus(allRows, "Rejected")})`} onClick={() => setLocation("memoryRejected")} />
        </div>

        <div className="library-nav-group">
          <strong>Knowledge</strong>
          <NavButton active={location === "knowledgeAll"} label={`All Knowledge (${count(allRows, "Knowledge")})`} onClick={() => setLocation("knowledgeAll")} />
          <NavButton active={location === "knowledgeAstrology"} label="Astrology" onClick={() => setLocation("knowledgeAstrology")} />
          <NavButton active={location === "knowledgeVastu"} label="Vastu" onClick={() => setLocation("knowledgeVastu")} />
          <NavButton active={location === "knowledgeOccult"} label="Occult" onClick={() => setLocation("knowledgeOccult")} />
          <NavButton active={location === "knowledgeBooks"} label="Books" onClick={() => setLocation("knowledgeBooks")} />
          <NavButton active={location === "knowledgeDocuments"} label="Documents" onClick={() => setLocation("knowledgeDocuments")} />
        </div>

        <div className="library-nav-group">
          <strong>Imports</strong>
          <NavButton active={location === "importsAll"} label={`All Imports (${count(allRows, "Import")})`} onClick={() => setLocation("importsAll")} />
          <NavButton active={location === "importsPending"} label={`Pending Review (${countPendingImports(allRows)})`} onClick={() => setLocation("importsPending")} />
          <NavButton active={location === "importsImages"} label="Images" onClick={() => setLocation("importsImages")} />
          <NavButton active={location === "importsPdfs"} label="PDFs" onClick={() => setLocation("importsPdfs")} />
        </div>
      </aside>

      <main className="library-main">
        <div className="library-toolbar">
          <button type="button">New</button>
          <button type="button">Upload</button>
          <button type="button">Approve</button>
          <button type="button">Reject</button>
          <button type="button">Delete</button>
          <button type="button">Refresh</button>
          <input
            value={query}
            placeholder={`Search ${locations[location]}`}
            onChange={(event) => setQuery(event.target.value)}
          />
        </div>

        <div className="library-breadcrumb">Library &gt; {locations[location]}</div>

        <div className="library-table">
          <div className="library-table-head">
            <span>Name</span>
            <span>Type</span>
            <span>Category</span>
            <span>Status</span>
            <span>Source</span>
            <span>Modified</span>
          </div>

          {filteredRows.map((item) => (
            <button
              className={`library-table-row ${selected?.id === item.id ? "selected" : ""}`}
              key={item.id}
              type="button"
              onClick={() => setSelectedId(item.id)}
            >
              <span title={item.name}>{iconFor(item.type)} {item.name}</span>
              <span>{item.type}</span>
              <span>{item.category}</span>
              <span>{item.status}</span>
              <span>{item.source}</span>
              <span>{formatDate(item.modified)}</span>
            </button>
          ))}

          {filteredRows.length === 0 && (
            <div className="library-empty">
              No items found in {locations[location]}.
            </div>
          )}
        </div>
      </main>

      <aside className="library-preview">
        <h3>Preview</h3>

        {selected ? (
          <>
            <strong>{selected.name}</strong>

            <div className="library-preview-meta">
              <span>Type: {selected.type}</span>
              <span>Category: {selected.category}</span>
              <span>Status: {selected.status}</span>
              <span>Source: {selected.source}</span>
              <span>Modified: {formatDate(selected.modified)}</span>
            </div>

            <p className="library-preview-content">{selected.content}</p>

            <div className="library-preview-actions">
              <button type="button">Open</button>
              <button type="button">Edit</button>
              <button type="button">Copy</button>
            </div>
          </>
        ) : (
          <p>Select an item to see details.</p>
        )}
      </aside>
    </section>
  );
}

function NavButton({ active, label, onClick }: { active: boolean; label: string; onClick: () => void }) {
  return (
    <button className={active ? "active" : ""} type="button" onClick={onClick}>
      {label}
    </button>
  );
}

function matchesLocation(item: LibraryItem, location: LibraryLocation) {
  if (location === "quickAccess") return true;

  if (location === "memoryAll") return item.type === "Memory";
  if (location === "memorySuggested") return item.type === "Memory" && equalsAny(item.status, ["Suggested", "Pending"]);
  if (location === "memoryPermanent") return item.type === "Memory" && equalsAny(item.status, ["Approved", "Permanent"]);
  if (location === "memoryTemporary") return item.type === "Memory" && equalsAny(item.status, ["Temporary"]);
  if (location === "memoryRejected") return item.type === "Memory" && equalsAny(item.status, ["Rejected"]);

  if (location === "knowledgeAll") return item.type === "Knowledge";
  if (location === "knowledgeAstrology") return item.type === "Knowledge" && equalsAny(item.category, ["Astrology"]);
  if (location === "knowledgeVastu") return item.type === "Knowledge" && equalsAny(item.category, ["Vastu"]);
  if (location === "knowledgeOccult") return item.type === "Knowledge" && equalsAny(item.category, ["Occult"]);
  if (location === "knowledgeBooks") return item.type === "Knowledge" && equalsAny(item.category, ["Books"]);
  if (location === "knowledgeDocuments") return item.type === "Knowledge" && equalsAny(item.category, ["Documents"]);

  if (location === "importsAll") return item.type === "Import";
  if (location === "importsPending") return item.type === "Import" && !equalsAny(item.status, ["Saved", "Approved", "Rejected"]);
  if (location === "importsImages") return item.type === "Import" && equalsAny(item.source, ["Image"]);
  if (location === "importsPdfs") return item.type === "Import" && equalsAny(item.source, ["Pdf", "PDF"]);

  return true;
}

function equalsAny(value: string, options: string[]) {
  return options.some((option) => value.toLowerCase() === option.toLowerCase());
}

function count(rows: LibraryItem[], type: LibraryItem["type"]) {
  return rows.filter((row) => row.type === type).length;
}

function countMemoryStatus(rows: LibraryItem[], status: string) {
  return rows.filter((row) => row.type === "Memory" && equalsAny(row.status, [status])).length;
}

function countPendingImports(rows: LibraryItem[]) {
  return rows.filter((row) => row.type === "Import" && !equalsAny(row.status, ["Saved", "Approved", "Rejected"])).length;
}

function iconFor(type: LibraryItem["type"]) {
  if (type === "Memory") return "🧠";
  if (type === "Knowledge") return "📚";
  return "📥";
}

function titleFromText(text: string | undefined, fallback: string) {
  const cleaned = (text || "").replace(/\s+/g, " ").trim();

  if (!cleaned) {
    return fallback;
  }

  return cleaned.length > 80 ? `${cleaned.slice(0, 80)}...` : cleaned;
}

function formatDate(value?: string) {
  if (!value) return "-";

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  return date.toLocaleDateString();
}