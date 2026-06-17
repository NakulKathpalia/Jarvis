"use client";

import { useEffect, useState } from "react";
import { TopBar } from "./TopBar";
import { PanelCard } from "./ui/PanelCard";
import { StatusBadge } from "./ui/StatusBadge";
import { jarvisApi } from "@/lib/api";
import type { DiagnosticsResult } from "@/lib/types";

export function DiagnosticsPanel() {
  const [diagnostics, setDiagnostics] = useState<DiagnosticsResult | null>(null);

  useEffect(() => {
    jarvisApi.diagnostics().then(setDiagnostics).catch(() => setDiagnostics(null));
  }, []);

  return (
    <section className="tool-panel">
      <TopBar title="Diagnostics" subtitle="Local runtime health and paths" />

      {!diagnostics && (
        <PanelCard>
          <p className="text-jarvis-muted">Loading diagnostics...</p>
        </PanelCard>
      )}

      {diagnostics && (
        <div className="grid w-full max-w-5xl gap-4">
          <div className="grid gap-4 md:grid-cols-3">
            <HealthCard label="Ollama" healthy={diagnostics.ollama.healthy} message={diagnostics.ollama.message} />
            <HealthCard label="Whisper" healthy={diagnostics.whisper.healthy} message={diagnostics.whisper.message} />
            <HealthCard label="Piper" healthy={diagnostics.piper.healthy} message={diagnostics.piper.message} />
          </div>

          <PanelCard className="grid gap-4">
            <h3 className="text-lg font-black text-jarvis-text">Platform</h3>
            <PathRow label="Current" value={diagnostics.platform} />
            <PathRow label="App Data" value={diagnostics.appDataPath} />
            <PathRow label="Memory" value={diagnostics.memoryPath} />
            <PathRow label="Logs" value={diagnostics.logsPath} />
            <PathRow label="Screenshots" value={diagnostics.screenshotPath} />
            <PathRow label="Generated Audio" value={diagnostics.generatedAudioPath} />
          </PanelCard>

          <PanelCard className="grid gap-3">
            <h3 className="text-lg font-black text-jarvis-text">Warnings</h3>
            {diagnostics.warnings.length === 0 ? (
              <p className="text-sm text-jarvis-muted">No configuration warnings.</p>
            ) : (
              diagnostics.warnings.map((warning) => (
                <p className="rounded-2xl border border-jarvis-amber/30 bg-jarvis-amber/10 p-3 text-sm text-jarvis-amber" key={warning}>
                  {warning}
                </p>
              ))
            )}
          </PanelCard>
        </div>
      )}
    </section>
  );
}

function HealthCard({ label, healthy, message }: { label: string; healthy: boolean; message: string }) {
  return (
    <PanelCard className="grid gap-3">
      <div className="flex items-center justify-between gap-3">
        <h3 className="text-lg font-black text-jarvis-text">{label}</h3>
        <StatusBadge tone={healthy ? "green" : "amber"}>{healthy ? "Ready" : "Check"}</StatusBadge>
      </div>
      <p className="text-sm leading-6 text-jarvis-muted">{message}</p>
    </PanelCard>
  );
}

function PathRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="grid gap-2 rounded-2xl border border-jarvis-border bg-white/[0.035] p-3 md:grid-cols-[160px_minmax(0,1fr)]">
      <strong className="text-sm text-jarvis-muted">{label}</strong>
      <span className="break-all font-mono text-xs text-jarvis-text">{value}</span>
    </div>
  );
}
