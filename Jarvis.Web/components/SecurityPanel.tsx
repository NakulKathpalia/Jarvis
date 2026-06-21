import type { AssistantInputResponse, InteractionStatusResult } from "@/lib/types";

type SecurityPanelProps = {
  interactionStatus: InteractionStatusResult | null;
  pendingAssistantCommand: { message: string; command: string; target: string } | null;
  lastAssistantAction: AssistantInputResponse | null;
};

const pipeline = [
  "Input validation",
  "Intent parsing",
  "Risk classification",
  "Permission check",
  "Confirmation gate",
  "Audit logging"
];

export function SecurityPanel({
  interactionStatus,
  pendingAssistantCommand,
  lastAssistantAction
}: SecurityPanelProps) {
  const risk = pendingAssistantCommand
    ? "Confirmation required"
    : lastAssistantAction?.type === "command"
      ? "Command checked"
      : "Idle";

  const recent = [
    interactionStatus?.lastCommandParsed?.message,
    interactionStatus?.lastAction?.message,
    interactionStatus?.lastError?.message
  ].filter(Boolean) as string[];

  return (
    <section className="tool-panel">
      <div className="page-header">
        <h2>Security</h2>
        <p>Local commands pass through Jarvis Security before execution.</p>
      </div>

      <div className="simple-grid">
        <article className="simple-card">
          <h3>Pending Confirmation</h3>
          <p>{pendingAssistantCommand ? pendingAssistantCommand.message : "No command is waiting for confirmation."}</p>
        </article>
        <article className="simple-card">
          <h3>Risk Level Summary</h3>
          <p>{risk}</p>
        </article>
      </div>

      <div className="simple-grid">
        {pipeline.map((item) => (
          <article className="simple-card" key={item}>
            <h3>{item}</h3>
            <p>Enabled in the backend security flow.</p>
          </article>
        ))}
      </div>

      <section className="simple-card logs-card">
        <h3>Recent Audit Activity</h3>
        {recent.length === 0 && <p>No recent security activity.</p>}
        {recent.map((item, index) => (
          <div className="log-row" key={`${item}-${index}`}>
            <strong>Security event</strong>
            <p>{item}</p>
          </div>
        ))}
      </section>
    </section>
  );
}
