"use client";

import { FormEvent, useState } from "react";
import { TopBar } from "./TopBar";
import { jarvisApi } from "@/lib/api";

type FilesPanelProps = {
  onToast: (message: string) => void;
};

export function FilesPanel({ onToast }: FilesPanelProps) {
  const [query, setQuery] = useState("");
  const [files, setFiles] = useState<string[]>([]);
  const [isIndexing, setIsIndexing] = useState(false);

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
      return;
    }

    setFiles(await jarvisApi.searchFiles(query.trim()));
  }

  return (
    <section className="tool-panel">
      <TopBar
        title="Files"
        subtitle="Index and search local paths"
        action={
          <button className="soft-button" type="button" onClick={indexFiles} disabled={isIndexing}>
            {isIndexing ? "Indexing" : "Index"}
          </button>
        }
      />

      <form className="tool-form" onSubmit={searchFiles}>
        <input
          value={query}
          placeholder="Search indexed files"
          onChange={(event) => setQuery(event.target.value)}
        />
        <button type="submit" disabled={!query.trim()}>
          Find
        </button>
      </form>

      <div className="item-list">
        {files.length === 0 && <div className="list-empty">Index files, then search here.</div>}
        {files.map((file) => (
          <article className="list-card file-card" key={file}>
            {file}
          </article>
        ))}
      </div>
    </section>
  );
}
