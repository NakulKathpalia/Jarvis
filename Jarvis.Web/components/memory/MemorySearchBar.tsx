"use client";

import { FormEvent } from "react";

type MemorySearchBarProps = {
  query: string;
  category: string;
  tag: string;
  memoryType: string;
  reviewStatus: string;
  minImportance: string;
  minConfidence: string;
  isSearching: boolean;
  searchActive: boolean;
  onQueryChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onTagChange: (value: string) => void;
  onMemoryTypeChange: (value: string) => void;
  onReviewStatusChange: (value: string) => void;
  onMinImportanceChange: (value: string) => void;
  onMinConfidenceChange: (value: string) => void;
  onSearch: (event?: FormEvent<HTMLFormElement>) => Promise<void>;
  onReset: () => void;
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
  "Goals",
  "Health",
  "General"
];

export function MemorySearchBar({
  query,
  category,
  tag,
  memoryType,
  reviewStatus,
  minImportance,
  minConfidence,
  isSearching,
  searchActive,
  onQueryChange,
  onCategoryChange,
  onTagChange,
  onMemoryTypeChange,
  onReviewStatusChange,
  onMinImportanceChange,
  onMinConfidenceChange,
  onSearch,
  onReset
}: MemorySearchBarProps) {
  return (
    <form className="memory-search-bar" onSubmit={onSearch}>
      <input
        value={query}
        placeholder="Search memory text"
        onChange={(event) => onQueryChange(event.target.value)}
      />
      <select value={category} onChange={(event) => onCategoryChange(event.target.value)}>
        <option value="">All categories</option>
        {memoryCategories.map((categoryOption) => (
          <option key={categoryOption} value={categoryOption}>
            {categoryOption}
          </option>
        ))}
      </select>
      <input
        value={tag}
        placeholder="Tag filter"
        onChange={(event) => onTagChange(event.target.value)}
      />
      <select value={memoryType} onChange={(event) => onMemoryTypeChange(event.target.value)}>
        <option value="">All types</option>
        <option value="TemporaryContext">Temporary</option>
        <option value="SuggestedMemory">Suggested</option>
        <option value="PermanentMemory">Permanent</option>
      </select>
      <select value={reviewStatus} onChange={(event) => onReviewStatusChange(event.target.value)}>
        <option value="">All statuses</option>
        <option value="Pending">Pending</option>
        <option value="Approved">Approved</option>
        <option value="Rejected">Rejected</option>
      </select>
      <input
        type="number"
        min={1}
        max={10}
        value={minImportance}
        placeholder="Min importance"
        onChange={(event) => onMinImportanceChange(event.target.value)}
      />
      <input
        type="number"
        min={1}
        max={10}
        value={minConfidence}
        placeholder="Min confidence"
        onChange={(event) => onMinConfidenceChange(event.target.value)}
      />
      <button type="submit" disabled={!searchActive || isSearching}>
        {isSearching ? "Searching" : "Search"}
      </button>
      <button type="button" className="soft-button" onClick={onReset} disabled={!searchActive}>
        Reset
      </button>
    </form>
  );
}
