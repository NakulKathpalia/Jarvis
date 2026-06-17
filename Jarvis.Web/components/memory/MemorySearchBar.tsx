"use client";

import { FormEvent } from "react";

type MemorySearchBarProps = {
  query: string;
  category: string;
  tag: string;
  isSearching: boolean;
  searchActive: boolean;
  onQueryChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onTagChange: (value: string) => void;
  onSearch: (event?: FormEvent<HTMLFormElement>) => Promise<void>;
  onReset: () => void;
};

export function MemorySearchBar({
  query,
  category,
  tag,
  isSearching,
  searchActive,
  onQueryChange,
  onCategoryChange,
  onTagChange,
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
      <button type="submit" disabled={!searchActive || isSearching}>
        {isSearching ? "Searching" : "Search"}
      </button>
      <button type="button" className="soft-button" onClick={onReset} disabled={!searchActive}>
        Reset
      </button>
    </form>
  );
}
