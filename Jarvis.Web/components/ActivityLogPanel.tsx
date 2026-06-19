"use client";

import { useEffect, useMemo, useState } from "react";
import { TopBar } from "./TopBar";
import { PanelCard } from "./ui/PanelCard";
import { StatusBadge } from "./ui/StatusBadge";
import { jarvisApi } from "@/lib/api";
import type { InteractionLogEntry } from "@/lib/types";

type ActivityFilter = "all" | "chat" | "voice" | "commands" | "errors";

const filters: Array<{ key: ActivityFilter; label: string }> = [
  { key: "all", label: "All" },
  { key: "chat", label: "Chat" },
  { key: "voice", label: "Voice" },
  { key: "commands", label: "Commands" },
  { key: "errors", label: "Errors" }
];

export function ActivityLogPanel() {
  const [logs, setLogs] = useState<InteractionLogEntry[]>([]);
  const [filter, setFilter] = useState<ActivityFilter>("all");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  async function loadLogs() {
    setIsLoading(true);
    try {
      setLogs(await jarvisApi.interactionLogs(100));
      setError("");
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : "Unable to load activity logs");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadLogs();
    const timer = window.setInterval(() => void loadLogs(), 5000);
    return () => window.clearInterval(timer);
  }, []);

  const visibleLogs = useMemo(() => {
    return logs.filter((log) => {
      if (filter === "all") return true;
      if (filter === "chat") return log.source === "Chat";
      if (filter === "voice") return log.source === "Voice";
      if (filter === "commands") return log.type === "CommandParsing" || log.type === "CommandExecution" || log.type === "Confirmation";
      return log.status === "Failed" || log.type === "Error" || Boolean(log.error);
    });
  }, [filter, logs]);

  async function clearLogs() {
    await jarvisApi.clearInteractionLogs();
    setLogs([]);
  }

  return (
    <section className="tool-panel">
      <TopBar
        title="Activity"
        subtitle="Central Jarvis interaction timeline"
        action={
          <div className="flex flex-wrap gap-2">
            <button className="soft-button" type="button" onClick={loadLogs} disabled={isLoading}>
              {isLoading ? "Refreshing" : "Refresh"}
            </button>
            <button className="danger-button" type="button" onClick={clearLogs}>
              Clear Logs
            </button>
          </div>
        }
      />

      <div className="mx-auto mb-4 flex w-full max-w-5xl flex-wrap gap-2">
        {filters.map((item) => (
          <button
            key={item.key}
            type="button"
            className={filter === item.key ? "primary-button" : "soft-button"}
            onClick={() => setFilter(item.key)}
          >
            {item.label}
          </button>
        ))}
      </div>

      {error && (
        <PanelCard className="mx-auto mb-4 w-full max-w-5xl border-jarvis-danger/40 bg-jarvis-danger/10">
          <p className="text-sm text-rose-200">{error}</p>
        </PanelCard>
      )}

      <div className="mx-auto grid w-full max-w-5xl gap-3">
        {visibleLogs.length === 0 && (
          <PanelCard>
            <p className="text-sm text-jarvis-muted">No activity logs match this filter yet.</p>
          </PanelCard>
        )}

        {visibleLogs.map((log) => (
          <article key={log.id} className="rounded-2xl border border-jarvis-border bg-jarvis-card/80 p-4 shadow-card">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <StatusBadge tone={toneForStatus(log.status)}>{log.status}</StatusBadge>
                  <strong className="text-sm text-jarvis-text">{log.source} / {log.type}</strong>
                  <span className="text-xs text-jarvis-faint">{log.stage}</span>
                </div>
                <p className="mt-2 text-sm leading-6 text-jarvis-muted">{log.message || "No message."}</p>
              </div>
              <time className="text-xs text-jarvis-faint">{new Date(log.timestampUtc).toLocaleString()}</time>
            </div>

            <div className="mt-3 grid gap-2 md:grid-cols-2">
              {log.input && <Preview label="Input" value={log.input} />}
              {log.output && <Preview label="Output" value={log.output} />}
              {log.error && <Preview label="Error" value={log.error} danger />}
            </div>
          </article>
        ))}
      </div>
    </section>
  );
}

function Preview({ label, value, danger = false }: { label: string; value: string; danger?: boolean }) {
  return (
    <div className={`rounded-2xl border p-3 ${danger ? "border-jarvis-danger/40 bg-jarvis-danger/10" : "border-jarvis-border bg-black/20"}`}>
      <div className="mb-1 text-xs font-black uppercase text-jarvis-faint">{label}</div>
      <p className={`line-clamp-4 break-words text-sm leading-6 ${danger ? "text-rose-200" : "text-jarvis-muted"}`}>{value}</p>
    </div>
  );
}

function toneForStatus(status: string) {
  if (status === "Success") return "green";
  if (status === "Failed") return "red";
  if (status === "Pending") return "amber";
  return "neutral";
}
