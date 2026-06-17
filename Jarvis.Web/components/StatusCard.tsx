import type { JarvisStatus } from "@/lib/types";

type StatusCardProps = {
  status: JarvisStatus | null;
  memoryCount: number;
};

export function StatusCard({ status, memoryCount }: StatusCardProps) {
  return (
    <section className="status-card" aria-label="Jarvis status">
      <div className="status-row">
        <span>Status</span>
        <strong className={status?.online ? "online" : "offline"}>
          {status?.online ? "Online" : "Offline"}
        </strong>
      </div>
      <div className="status-row">
        <span>Model</span>
        <strong title={status?.model ?? ""}>{status?.model ?? "-"}</strong>
      </div>
      <div className="status-row">
        <span>Memory</span>
        <strong>{memoryCount}</strong>
      </div>
    </section>
  );
}
