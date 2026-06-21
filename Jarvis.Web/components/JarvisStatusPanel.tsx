import type { AssistantInputResponse, InteractionStatusResult, JarvisStatus } from "@/lib/types";

type JarvisStatusPanelProps = {
  status: JarvisStatus | null;
  interactionStatus: InteractionStatusResult | null;
  pendingAssistantCommand: { message: string; command: string; target: string } | null;
  lastAssistantAction: AssistantInputResponse | null;
};

export function JarvisStatusPanel({
  status,
  interactionStatus,
  pendingAssistantCommand,
  lastAssistantAction
}: JarvisStatusPanelProps) {
  const lastRisk = pendingAssistantCommand
    ? "Dangerous"
    : lastAssistantAction?.requiresConfirmation
      ? "Dangerous"
      : lastAssistantAction?.type === "command"
        ? "Medium"
        : "Safe";

  const recent = [
    interactionStatus?.lastAction?.message,
    interactionStatus?.lastCommandParsed?.message,
    lastAssistantAction?.message
  ].filter(Boolean).slice(0, 4) as string[];

  return (
    <aside className="jarvis-status-panel">
      <section className="status-card">
        <h3>System Status</h3>
        <StatusRow label="CPU" value="Idle" />
        <StatusRow label="RAM" value="Available" />
        <StatusRow label="GPU" value="Local" />
        <StatusRow label="Internet" value={status?.online ? "Connected" : "Unknown"} />
      </section>

      <section className="status-card">
        <h3>Voice</h3>
        <StatusRow label="Wake word" value="jarvis" />
        <StatusRow label="State" value="Idle" />
      </section>

      <section className="status-card accent">
        <h3>Security</h3>
        <StatusRow label="Secure Mode" value="On" />
        <StatusRow label="Pending Confirmation" value={pendingAssistantCommand ? "yes run it" : "None"} />
        <StatusRow label="Last Risk Level" value={lastRisk} />
      </section>

      <section className="status-card">
        <h3>Recent Actions</h3>
        {recent.length === 0 && <p className="status-empty">No recent actions.</p>}
        {recent.map((item, index) => (
          <p className="recent-action" key={`${item}-${index}`}>{item}</p>
        ))}
      </section>
    </aside>
  );
}

function StatusRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="status-row">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}
