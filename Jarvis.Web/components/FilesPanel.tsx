"use client";

import { FormEvent, useState } from "react";
import { TopBar } from "./TopBar";
import { FileResultCard } from "./files/FileResultCard";
import { jarvisApi } from "@/lib/api";
import type { FileSearchResult } from "@/lib/types";

type FilesPanelProps = {
  onToast: (message: string) => void;
};

type FileOpenAction = "file" | "folder";

export function FilesPanel({ onToast }: FilesPanelProps) {
  const [query, setQuery] = useState("");
  const [files, setFiles] = useState<FileSearchResult[]>([]);
  const [isIndexing, setIsIndexing] = useState(false);
  const [isSearching, setIsSearching] = useState(false);
  const [openingPath, setOpeningPath] = useState<string | null>(null);

  async function indexFiles() {
    setIsIndexing(true);
    try {
      const result = await jarvisApi.indexFiles();
      onToast(`Indexed ${result.count} files`);
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Index failed");
    } finally {
      setIsIndexing(false);
    }
  }

  async function searchFiles(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!query.trim()) {
      setFiles([]);
      return;
    }

    setIsSearching(true);
    try {
      setFiles(await jarvisApi.searchFilesDetailed(query.trim()));
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Search failed");
    } finally {
      setIsSearching(false);
    }
  }

  async function openPath(action: FileOpenAction, path: string) {
    setOpeningPath(path);
    try {
      const result = action === "file"
        ? await jarvisApi.openFile(path)
        : await jarvisApi.openContainingFolder(path);
      onToast(result.message);
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Open action failed");
    } finally {
      setOpeningPath(null);
    }
  }

  return (
    <section className="tool-panel">
      <TopBar
        title="Files"
        subtitle="Index, search, and open local files"
        action={
          <button className="soft-button" type="button" onClick={indexFiles} disabled={isIndexing}>
            {isIndexing ? "Indexing" : "Index"}
          </button>
        }
      />

      <form className="tool-form" onSubmit={searchFiles}>
        <input
          value={query}
          placeholder="Search indexed file names or content"
          onChange={(event) => setQuery(event.target.value)}
        />
        <button type="submit" disabled={!query.trim() || isSearching}>
          {isSearching ? "Searching" : "Search"}
        </button>
      </form>

      <div className="item-list">
        {files.length === 0 && <div className="list-empty">Index files, then search file names or content here.</div>}

        {files.map((file) => (
          <FileResultCard
            key={file.path}
            file={file}
            isOpening={openingPath === file.path}
            onOpenFile={(path) => openPath("file", path)}
            onOpenFolder={(path) => openPath("folder", path)}
          />
        ))}
      </div>
    </section>
  );
}
