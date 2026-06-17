"use client";

import { FormEvent, useEffect, useState } from "react";
import { TopBar } from "./TopBar";
import { jarvisApi } from "@/lib/api";
import type { PcCommandCatalogItem, PcCommandExecutionResult, PcCommandLogEntry } from "@/lib/types";

type ControlPanelProps = {
  onToast: (message: string) => void;
};

export function ControlPanel({ onToast }: ControlPanelProps) {
  const [input, setInput] = useState("");
  const [catalog, setCatalog] = useState<PcCommandCatalogItem[]>([]);
  const [logs, setLogs] = useState<PcCommandLogEntry[]>([]);
  const [pending, setPending] = useState<PcCommandExecutionResult | null>(null);
  const [message, setMessage] = useState("");
  const [isRunning, setIsRunning] = useState(false);

  useEffect(() => {
    refreshControlData().catch((error: Error) => onToast(error.message));
  }, [onToast]);

  async function refreshControlData() {
    const [nextCatalog, nextLogs] = await Promise.all([
      jarvisApi.commandCatalog(),
      jarvisApi.commandLogs()
    ]);
    setCatalog(nextCatalog);
    setLogs(nextLogs);
  }

  async function executeCommand(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = input.trim();
    if (!trimmed) {
      return;
    }

    setIsRunning(true);
    try {
      const result = await jarvisApi.executeCommand(trimmed);
      setMessage(result.message);
      setPending(result.requiresConfirmation ? result : null);
      if (!result.requiresConfirmation) {
        setInput("");
      }
      await refreshControlData();
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Command failed");
    } finally {
      setIsRunning(false);
    }
  }

  async function confirmCommand() {
    const confirmationId = pending?.confirmationId ?? pending?.confirmationToken;
    if (!confirmationId) {
      return;
    }

    setIsRunning(true);
    try {
      const result = await jarvisApi.confirmCommand(confirmationId);
      setMessage(result.message);
      setPending(null);
      setInput("");
      await refreshControlData();
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Confirmation failed");
    } finally {
      setIsRunning(false);
    }
  }

  function cancelPending() {
    setPending(null);
    setMessage("Command cancelled locally.");
  }

  return (
    <section className="tool-panel">
      <TopBar title="Control" subtitle="Known local PC commands with safety checks" />

      <form className="tool-form" onSubmit={executeCommand}>
        <input
          value={input}
          placeholder="Try: search web for local AI models"
          onChange={(event) => setInput(event.target.value)}
        />
        <button type="submit" disabled={isRunning || !input.trim()}>
          {isRunning ? "Running" : "Run"}
        </button>
      </form>

      {message && <div className="control-message">{message}</div>}

      {pending && (
        <div className="control-confirmation">
          <div>
            <strong>Confirmation required</strong>
            <p>{pending.command} {pending.target ? `- ${pending.target}` : ""}</p>
          </div>
          <div className="memory-actions">
            <button className="primary-button" type="button" onClick={confirmCommand} disabled={isRunning}>
              Confirm
            </button>
            <button className="soft-button" type="button" onClick={cancelPending}>
              Cancel
            </button>
          </div>
        </div>
      )}

      <div className="control-grid">
        <section className="control-section">
          <h3>Catalog</h3>
          <div className="item-list">
            {catalog.map((item) => (
              <article className="list-card control-card" key={item.command}>
                <div className="control-card-head">
                  <strong>{item.command}</strong>
                  <span className={`control-pill ${item.safetyLevel.toLowerCase()}`}>{item.safetyLevel}</span>
                </div>
                <p>{item.description}</p>
                <span>{item.examples.join(" | ")}</span>
              </article>
            ))}
          </div>
        </section>

        <section className="control-section">
          <h3>Recent Logs</h3>
          <div className="item-list">
            {logs.length === 0 && <div className="list-empty">No command logs yet.</div>}
            {logs.map((log) => (
              <article className="list-card control-card" key={log.id}>
                <div className="control-card-head">
                  <strong>{log.parsedCommand}</strong>
                  <span className={`control-pill ${log.status.toLowerCase()}`}>{log.status}</span>
                </div>
                <p>{log.originalInput}</p>
                {log.target && <span>{log.target}</span>}
                <span>{log.resultMessage}</span>
                <span>{new Date(log.timestampUtc).toLocaleString()}</span>
              </article>
            ))}
          </div>
        </section>
      </div>
    </section>
  );
}
