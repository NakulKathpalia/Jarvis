"use client";

import { FormEvent } from "react";

type MemorySearchBarProps = {
  query: string;
  category: string;
  tag: string;
  memoryType: string;
  reviewStatus: string;
  isSearching: boolean;
  searchActive: boolean;
  onQueryChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onTagChange: (value: string) => void;
  onMemoryTypeChange: (value: string) => void;
  onReviewStatusChange: (value: string) => void;
  onSearch: (event?: FormEvent<HTMLFormElement>) => Promise<void>;
  onReset: () => void;
};

export function MemorySearchBar({
  query,
  category,
  tag,
  memoryType,
  reviewStatus,
  isSearching,
  searchActive,
  onQueryChange,
  onCategoryChange,
  onTagChange,
  onMemoryTypeChange,
  onReviewStatusChange,
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
      <input
        value={category}
        placeholder="Category filter"
        onChange={(event) => onCategoryChange(event.target.value)}
      />
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
      <button type="submit" disabled={!searchActive || isSearching}>
        {isSearching ? "Searching" : "Search"}
      </button>
      <button type="button" className="soft-button" onClick={onReset} disabled={!searchActive}>
        Reset
      </button>
    </form>
  );
}
