"use client";

import { FormEvent, useEffect, useState } from "react";
import { jarvisApi } from "@/lib/api";
import type { PcCommandExecutionResult, PcCommandLogEntry } from "@/lib/types";

type ControlPanelProps = {
  onToast: (message: string) => void;
};

const commandSections = [
  { title: "Apps", actions: ["open chrome", "open notepad"] },
  { title: "Websites", actions: ["open youtube", "open github.com"] },
  { title: "Files", actions: ["search files for resume", "take screenshot"] },
  { title: "System Actions", actions: ["shutdown computer", "restart computer"] }
];

export function ControlPanel({ onToast }: ControlPanelProps) {
  const [input, setInput] = useState("");
  const [logs, setLogs] = useState<PcCommandLogEntry[]>([]);
  const [pending, setPending] = useState<PcCommandExecutionResult | null>(null);
  const [message, setMessage] = useState("");
  const [isRunning, setIsRunning] = useState(false);

  useEffect(() => {
    refreshLogs().catch((error: Error) => onToast(error.message));
  }, [onToast]);

  async function refreshLogs() {
    setLogs(await jarvisApi.commandLogs());
  }

  async function runCommand(command: string) {
    setIsRunning(true);
    try {
      const result = await jarvisApi.executeCommand(command);
      setMessage(result.message);
      setPending(result.requiresConfirmation ? result : null);
      await refreshLogs();
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Command failed");
    } finally {
      setIsRunning(false);
    }
  }

  async function executeCommand(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = input.trim();
    if (!trimmed) return;
    await runCommand(trimmed);
    if (!pending) setInput("");
  }

  async function confirmCommand() {
    const confirmationId = pending?.confirmationId ?? pending?.confirmationToken ?? "yes run it";
    setIsRunning(true);
    try {
      const result = await jarvisApi.confirmCommand(confirmationId);
      setMessage(result.message);
      setPending(null);
      setInput("");
      await refreshLogs();
    } catch (error) {
      onToast(error instanceof Error ? error.message : "Confirmation failed");
    } finally {
      setIsRunning(false);
    }
  }

  return (
    <section className="tool-panel">
      <div className="page-header">
        <h2>PC Control</h2>
        <p>Every action is sent to Jarvis backend security before execution.</p>
      </div>

      <form className="command-form" onSubmit={executeCommand}>
        <input value={input} placeholder="Type a command, e.g. open chrome" onChange={(event) => setInput(event.target.value)} />
        <button type="submit" disabled={isRunning || !input.trim()}>{isRunning ? "Checking" : "Send"}</button>
      </form>

      {message && <div className="control-message">{message}</div>}
      {pending && (
        <div className="confirmation-card compact">
          <strong>This action is risky. Type: yes run it</strong>
          <p>{pending.command} {pending.target}</p>
          <div>
            <button type="button" onClick={confirmCommand} disabled={isRunning}>yes run it</button>
            <button type="button" onClick={() => setPending(null)}>Cancel</button>
          </div>
        </div>
      )}

      <div className="simple-grid">
        {commandSections.map((section) => (
          <article className={section.title === "System Actions" ? "simple-card danger-zone-card" : "simple-card"} key={section.title}>
            <h3>{section.title}</h3>
            {section.title === "System Actions" && <p>These actions require confirmation before execution.</p>}
            <div className="action-list">
              {section.actions.map((action) => (
                <button type="button" key={action} onClick={() => void runCommand(action)} disabled={isRunning}>
                  {action}
                </button>
              ))}
            </div>
          </article>
        ))}
      </div>

      <section className="simple-card logs-card">
        <h3>Recent Actions</h3>
        {logs.length === 0 && <p>No command logs yet.</p>}
        {logs.slice(0, 8).map((log) => (
          <div className="log-row" key={log.id}>
            <strong>{log.parsedCommand}</strong>
            <span>{log.status}</span>
            <p>{log.resultMessage}</p>
          </div>
        ))}
      </section>
    </section>
  );
}
