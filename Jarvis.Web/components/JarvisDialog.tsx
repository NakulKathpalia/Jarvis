"use client";

import type React from "react";

type JarvisDialogProps = {
  open: boolean;
  title: string;
  subtitle?: string;
  size?: "md" | "lg" | "xl";
  children: React.ReactNode;
  onClose: () => void;
};

export function JarvisDialog({ open, title, subtitle, size = "lg", children, onClose }: JarvisDialogProps) {
  if (!open) {
    return null;
  }

  return (
    <div className="jarvis-dialog-backdrop" role="presentation" onMouseDown={onClose}>
      <section
        aria-modal="true"
        className={`jarvis-dialog jarvis-dialog-${size}`}
        role="dialog"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="jarvis-dialog-header">
          <div>
            <h2>{title}</h2>
            {subtitle && <p>{subtitle}</p>}
          </div>
          <button aria-label={`Close ${title}`} className="dialog-close-button" type="button" onClick={onClose}>
            x
          </button>
        </header>
        <div className="jarvis-dialog-body">{children}</div>
      </section>
    </div>
  );
}
