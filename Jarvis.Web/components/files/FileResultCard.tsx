"use client";

import type { FileSearchResult } from "@/lib/types";
import { formatFileSize } from "./fileFormatting";

type FileResultCardProps = {
  file: FileSearchResult;
  isOpening: boolean;
  onOpenFile: (path: string) => Promise<void>;
  onOpenFolder: (path: string) => Promise<void>;
};

export function FileResultCard({ file, isOpening, onOpenFile, onOpenFolder }: FileResultCardProps) {
  return (
    <article className="list-card file-card">
      <div className="file-card-head">
        <strong>{file.fileName}</strong>
        <span className="file-match-pill">{file.matchType}</span>
      </div>

      <span className="file-path">{file.relativePath}</span>
      {file.snippet && <p className="file-snippet">{file.snippet}</p>}

      <span>
        {formatFileSize(file.sizeBytes)} - {new Date(file.lastWriteTimeUtc).toLocaleString()}
      </span>

      <div className="memory-actions">
        <button type="button" className="soft-button" onClick={() => onOpenFile(file.path)} disabled={isOpening}>
          Open file
        </button>
        <button type="button" className="soft-button" onClick={() => onOpenFolder(file.path)} disabled={isOpening}>
          Open folder
        </button>
      </div>
    </article>
  );
}
